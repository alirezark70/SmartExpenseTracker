using Microsoft.EntityFrameworkCore;
using SmartExpenseTracker.Core.ApplicationService.Contracts.Persistence;
using SmartExpenseTracker.Core.Domain.Contracts.Common;
using SmartExpenseTracker.Core.Domain.DomainModels.Common;
using SmartExpenseTracker.Infra.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Infra.Persistence.Services.Base
{
    public class EfCoreRepository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
    {
        private readonly WriteDbContext _context;
        private readonly DbSet<TEntity> _dbSet;

        public EfCoreRepository(WriteDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<TEntity>();
        }

        public async Task<TEntity?> GetByIdAsync(
            object id,
            CancellationToken cancellationToken = default,
            params Expression<Func<TEntity, object>>[] includes)
        {
            var query = _dbSet.AsQueryable();

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.FirstOrDefaultAsync(
                e => e.Id.Equals(id),
                cancellationToken);
        }

        public async Task<TEntity?> GetBySpecAsync(
            ISpecification<TEntity> specification,
            CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<TEntity>> ListAsync(
            ISpecification<TEntity> specification,
            CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<TEntity>> ListAllAsync(
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<int> CountAsync(
            ISpecification<TEntity> specification,
            CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification)
                .CountAsync(cancellationToken);
        }

        public async Task<bool> ExistsAsync(
            ISpecification<TEntity> specification,
            CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification)
                .AnyAsync(cancellationToken);
        }

        public async Task<TEntity> AddAsync(
            TEntity entity,
            CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
            return entity;
        }

        public async Task AddRangeAsync(
            IEnumerable<TEntity> entities,
            CancellationToken cancellationToken = default)
        {
            await _dbSet.AddRangeAsync(entities, cancellationToken);
        }

        public void Update(TEntity entity)
        {
            _dbSet.Update(entity);
        }

        public void UpdateRange(IEnumerable<TEntity> entities)
        {
            _dbSet.UpdateRange(entities);
        }

        public void Remove(TEntity entity)
        {
            _dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<TEntity> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        // Bulk Operations using EFCore.BulkExtensions
        public async Task BulkInsertAsync(
            IList<TEntity> entities,
            CancellationToken cancellationToken = default)
        {
            await _context.BulkInsertAsync(entities, cancellationToken: cancellationToken);
        }

        public async Task BulkUpdateAsync(
            IList<TEntity> entities,
            CancellationToken cancellationToken = default)
        {
            await _context.BulkUpdateAsync(entities, cancellationToken: cancellationToken);
        }

        public async Task BulkDeleteAsync(
            IList<TEntity> entities,
            CancellationToken cancellationToken = default)
        {
            await _context.BulkDeleteAsync(entities, cancellationToken: cancellationToken);
        }

        public void Attach(TEntity entity)
        {
            _dbSet.Attach(entity);
        }

        public void Detach(TEntity entity)
        {
            _context.Entry(entity).State = EntityState.Detached;
        }

        public EntityState GetEntityState(TEntity entity)
        {
            return _context.Entry(entity).State;
        }

        public void SetEntityState(TEntity entity, EntityState state)
        {
            _context.Entry(entity).State = state;
        }

        // Helper method to apply specification
        private IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> spec)
        {
            var query = _dbSet.AsQueryable();

            if (spec.Criteria != null)
            {
                query = query.Where(spec.Criteria);
            }

            // Apply includes
            query = spec.Includes.Aggregate(query,
                (current, include) => current.Include(include));

            // Apply string includes
            query = spec.IncludeStrings.Aggregate(query,
                (current, include) => current.Include(include));

            // Apply ordering
            if (spec.OrderBy != null)
            {
                query = query.OrderBy(spec.OrderBy);
            }
            else if (spec.OrderByDescending != null)
            {
                query = query.OrderByDescending(spec.OrderByDescending);
            }

            // Apply paging
            if (spec.IsPagingEnabled)
            {
                query = query.Skip(spec.Skip).Take(spec.Take);
            }

            // Apply distinct
            if (spec.IsDistinct)
            {
                query = query.Distinct();
            }

            // Apply tracking
            if (spec.AsNoTracking)
            {
                query = query.AsNoTracking();
            }

            return query;
        }
    }
}
