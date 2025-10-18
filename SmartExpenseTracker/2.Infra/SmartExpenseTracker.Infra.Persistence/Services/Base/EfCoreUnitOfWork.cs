using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using SmartExpenseTracker.Core.ApplicationService.Contracts;
using SmartExpenseTracker.Core.ApplicationService.Contracts.Base;
using SmartExpenseTracker.Core.ApplicationService.Contracts.Persistence;
using SmartExpenseTracker.Core.ApplicationService.Contracts.Persistence.Users;
using SmartExpenseTracker.Core.ApplicationService.Exceptions;
using SmartExpenseTracker.Core.Domain.Contracts.Common;
using SmartExpenseTracker.Core.Domain.DomainModels.Common;
using SmartExpenseTracker.Core.Domain.Events.Base;
using SmartExpenseTracker.Infra.Persistence.Context;
using SmartExpenseTracker.Infra.Persistence.Repositories.Users;
using System.Data;

namespace SmartExpenseTracker.Infra.Persistence.Services.Base
{
    public sealed class EfCoreUnitOfWork : IUnitOfWork
    {
        private readonly WriteDbContext _context;
        private readonly ILogger<EfCoreUnitOfWork> _logger;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly Dictionary<Type, object> _repositories;
        private readonly List<IDbContextTransaction> _transactions;
        private bool _disposed;
        private IDbContextTransaction? _currentTransaction; 
        // Specialized Repositories
        private IUserRepository? _userRepository;

        public EfCoreUnitOfWork(
            WriteDbContext context,
            ILogger<EfCoreUnitOfWork> logger,
            IDateTimeProvider dateTimeProvider,
            IUserRepository? userRepository)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _repositories = new Dictionary<Type, object>();
            _transactions = new List<IDbContextTransaction>();
            _userRepository = userRepository;
        }

        public bool HasActiveTransaction => _transactions.Any();

        public IUserRepository UserRepository
        {
            get
            {
                if (_userRepository == null)
                {
                    _userRepository = new UserRepository(_context);
                }
                return _userRepository;
            }
        }

        public IRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity
        {
            var type = typeof(TEntity);

            if (!_repositories.ContainsKey(type))
            {
                var repositoryType = typeof(EfCoreRepository<>).MakeGenericType(type);
                _repositories[type] = Activator.CreateInstance(repositoryType, _context)
                    ?? throw new InvalidOperationException($"Could not create repository for {type.Name}");
            }

            return (IRepository<TEntity>)_repositories[type];
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync(
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            CancellationToken cancellationToken = default)
        {
            var transaction = await _context.Database
                .BeginTransactionAsync(isolationLevel, cancellationToken);

            _transactions.Add(transaction);

            _logger.LogDebug("Transaction started with isolation level: {IsolationLevel}", isolationLevel);

            return transaction;
        }

        public async Task CommitTransactionAsync(
            IDbContextTransaction transaction,
            CancellationToken cancellationToken = default)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            if (!_transactions.Contains(transaction))
                throw new InvalidOperationException("Transaction is not managed by this unit of work");

            try
            {
                await SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogDebug("Transaction committed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error committing transaction");
                await RollbackTransactionAsync(transaction, cancellationToken);
                throw;
            }
            finally
            {
                _transactions.Remove(transaction);
                await transaction.DisposeAsync();
            }
        }

        public async Task RollbackTransactionAsync(
            IDbContextTransaction transaction,
            CancellationToken cancellationToken = default)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            try
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogDebug("Transaction rolled back");
            }
            finally
            {
                _transactions.Remove(transaction);
                await transaction.DisposeAsync();
            }
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // Update audit fields

                // Get domain events before saving
                var domainEvents = GetDomainEvents();

                var result = await _context.SaveChangesAsync(cancellationToken);

                // Dispatch domain events after successful save
                await DispatchDomainEvents(domainEvents);

                return result;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency exception occurred");
                throw new ConcurrencyException("The entity has been modified by another user", ex);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database update exception occurred");
                throw new DataAccessException("An error occurred while saving changes", ex);
            }
        }

        public async Task<int> SaveChangesAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await SaveChangesAsync(cancellationToken);
        }

        public async Task<T> ExecuteInTransactionAsync<T>(
            Func<Task<T>> action,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            CancellationToken cancellationToken = default)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await BeginTransactionAsync(isolationLevel, cancellationToken);
                try
                {
                    var result = await action();
                    await CommitTransactionAsync(transaction, cancellationToken);
                    return result;
                }
                catch
                {
                    await RollbackTransactionAsync(transaction, cancellationToken);
                    throw;
                }
            });
        }

        public async Task<int> ExecuteSqlRawAsync(
            string sql,
            CancellationToken cancellationToken = default,
            params object[] parameters)
        {
            return await _context.Database
                .ExecuteSqlRawAsync(sql, parameters, cancellationToken);
        }

        public async Task<List<T>> SqlQueryAsync<T>(
            string sql,
            CancellationToken cancellationToken = default,
            params object[] parameters) where T : class
        {
            return await _context.Set<T>()
                .FromSqlRaw(sql, parameters)
                .ToListAsync(cancellationToken);
        }

        public void ClearChangeTracker()
        {
            _context.ChangeTracker.Clear();
        }

        public async Task ReloadEntityAsync<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            await _context.Entry(entity).ReloadAsync();
        }


        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction != null)
            {
                _logger.LogWarning("Transaction already started. Skipping new transaction creation.");
                return;
            }

            _logger.LogDebug("Starting new transaction with default isolation level");

            _currentTransaction = await BeginTransactionAsync(
                IsolationLevel.ReadCommitted,
                cancellationToken);
        }


        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null)
            {
                throw new InvalidOperationException(
                    "No active transaction to commit. Call BeginTransactionAsync first.");
            }

            await CommitTransactionAsync(_currentTransaction, cancellationToken);
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null)
            {
                throw new InvalidOperationException(
                    "No active transaction to rollback. Call BeginTransactionAsync first.");
            }

            await RollbackTransactionAsync(_currentTransaction, cancellationToken);
        }

      


    
        private List<IDomainEvent> GetDomainEvents()
        {
            return _context.ChangeTracker
                .Entries<BaseEntity>()
                .SelectMany(x => x.Entity.DomainEvents)
                .ToList();
        }

        private async Task DispatchDomainEvents(List<IDomainEvent> domainEvents)
        {
            // This should be implemented with MediatR or your event dispatcher
            foreach (var domainEvent in domainEvents)
            {
                // await _mediator.Publish(domainEvent);
                _logger.LogDebug("Domain event dispatched: {EventType}", domainEvent.GetType().Name);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();
            Dispose(false);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                foreach (var transaction in _transactions)
                {
                    transaction?.Dispose();
                }
                _transactions.Clear();
                _repositories.Clear();
                _context?.Dispose();
            }

            _disposed = true;
        }

        private async ValueTask DisposeAsyncCore()
        {
            if (_disposed) return;

            foreach (var transaction in _transactions)
            {
                if (transaction != null)
                    await transaction.DisposeAsync();
            }

            if (_context != null)
                await _context.DisposeAsync();
        }
    }
}
