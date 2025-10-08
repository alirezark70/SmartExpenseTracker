using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SmartExpenseTracker.Core.ApplicationService.Contracts.Base;
using SmartExpenseTracker.Core.ApplicationService.Contracts.Persistence;
using SmartExpenseTracker.Core.Domain.Contracts.Common;
using SmartExpenseTracker.Core.Domain.DomainModels.Common;
using SmartExpenseTracker.Infra.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace SmartExpenseTracker.Infra.Persistence.Services
{
    public class EfCoreUnitOfWork : IUnitOfWork
    {
        private readonly WriteDbContext _context;
        private readonly Dictionary<Type, object> _repositories;
        private IDbContextTransaction? _transaction;
        private bool _disposed;

        public EfCoreUnitOfWork(WriteDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _repositories = new Dictionary<Type, object>();
        }

        public DbContext Context => _context;
        public bool HasActiveTransaction => _transaction != null;

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

        public async Task BeginTransactionAsync(
            CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
                throw new InvalidOperationException("Transaction has already been started.");

            _transaction = await _context.Database.BeginTransactionAsync(
                cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction == null)
                throw new InvalidOperationException("No active transaction to commit.");

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                await _transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction == null)
                throw new InvalidOperationException("No active transaction to rollback.");

            try
            {
                await _transaction.RollbackAsync(cancellationToken);
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
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

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _transaction?.Dispose();
                _repositories.Clear();
                _context?.Dispose();
            }

            _disposed = true;
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
            }

            if (_context != null)
            {
                await _context.DisposeAsync();
            }
        }

        
    }
}
