using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SmartExpenseTracker.Core.ApplicationService.Contracts.Persistence;
using SmartExpenseTracker.Core.ApplicationService.Contracts.Persistence.Users;
using SmartExpenseTracker.Core.Domain.DomainModels.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.ApplicationService.Contracts.Base
{
    public interface IUnitOfWork : IDisposable, IAsyncDisposable
    {
        // Repository Access
        IRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity;

        // Specialized Repositories
        IUserRepository UserRepository { get; }

        // Transaction Management
        Task<IDbContextTransaction> BeginTransactionAsync(
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            CancellationToken cancellationToken = default);

        Task CommitTransactionAsync(
            IDbContextTransaction transaction,
            CancellationToken cancellationToken = default);

        Task RollbackTransactionAsync(
            IDbContextTransaction transaction,
            CancellationToken cancellationToken = default);

        // Save Operations
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<int> SaveChangesAsync(string userId, CancellationToken cancellationToken = default);

        // Execution Strategy for Retries
        Task<T> ExecuteInTransactionAsync<T>(
            Func<Task<T>> action,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            CancellationToken cancellationToken = default);

        // Direct SQL Execution
        Task<int> ExecuteSqlRawAsync(
            string sql,
            CancellationToken cancellationToken = default,
            params object[] parameters);

        Task<List<T>> SqlQueryAsync<T>(
            string sql,
            CancellationToken cancellationToken = default,
            params object[] parameters) where T : class;

        // State Management
        bool HasActiveTransaction { get; }
        void ClearChangeTracker();
        Task ReloadEntityAsync<TEntity>(TEntity entity) where TEntity : BaseEntity;
    }
}
