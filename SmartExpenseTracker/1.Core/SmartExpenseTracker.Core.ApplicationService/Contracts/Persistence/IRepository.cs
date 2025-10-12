using Microsoft.EntityFrameworkCore;
using SmartExpenseTracker.Core.Domain.DomainModels.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace SmartExpenseTracker.Core.ApplicationService.Contracts.Persistence
{
    public interface IRepository<TEntity> where TEntity : BaseEntity
    {
        // Query Methods with Specification Pattern
        Task<TEntity?> GetByIdAsync(
            object id,
            CancellationToken cancellationToken = default,
            params Expression<Func<TEntity, object>>[] includes);

        Task<TEntity?> GetBySpecAsync(
            ISpecification<TEntity> specification,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<TEntity>> ListAsync(
            ISpecification<TEntity> specification,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<TEntity>> ListAllAsync(
            CancellationToken cancellationToken = default);

        Task<int> CountAsync(
            ISpecification<TEntity> specification,
            CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(
            ISpecification<TEntity> specification,
            CancellationToken cancellationToken = default);

        // Command Methods
        Task<TEntity> AddAsync(
            TEntity entity,
            CancellationToken cancellationToken = default);

        Task AddRangeAsync(
            IEnumerable<TEntity> entities,
            CancellationToken cancellationToken = default);

        void Update(TEntity entity);
        void UpdateRange(IEnumerable<TEntity> entities);

        void Remove(TEntity entity);
        void RemoveRange(IEnumerable<TEntity> entities);

      
        // Attach & Detach
        void Attach(TEntity entity);
        void Detach(TEntity entity);

        // Tracking
        EntityState GetEntityState(TEntity entity);
        void SetEntityState(TEntity entity, EntityState state);
    }
}

