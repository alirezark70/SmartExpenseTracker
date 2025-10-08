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
        // Query (محدود - فقط برای نیازهای داخلی command)
        Task<TEntity?> GetByIdAsync(
            object id,
            CancellationToken cancellationToken = default);

        Task<TEntity?> FindAsync(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default);

        // Commands
        Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

        void Update(TEntity entity);
        void UpdateRange(IEnumerable<TEntity> entities);

        void Remove(TEntity entity);
        void RemoveRange(IEnumerable<TEntity> entities);

        // Attach for update scenarios
        void Attach(TEntity entity);
        void AttachRange(IEnumerable<TEntity> entities);

        // Detach
        void Detach(TEntity entity);

        // Check tracking state
        EntityState GetEntityState(TEntity entity);
    }
}
