using SmartExpenseTracker.Core.ApplicationService.Contracts.Persistence;
using SmartExpenseTracker.Core.Domain.DomainModels.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Infra.Persistence.Services.Base
{
    public abstract class BaseSpecification<T> : ISpecification<T> where T : BaseEntity
    {
        public Expression<Func<T, bool>> Criteria { get; private set; } = null!;
        public List<Expression<Func<T, object>>> Includes { get; } = new();
        public List<string> IncludeStrings { get; } = new();
        public Expression<Func<T, object>>? OrderBy { get; private set; }
        public Expression<Func<T, object>>? OrderByDescending { get; private set; }
        public Expression<Func<T, object>>? GroupBy { get; private set; }

        public int Take { get; private set; }
        public int Skip { get; private set; }
        public bool IsPagingEnabled { get; private set; }
        public bool IsDistinct { get; private set; }
        public bool AsNoTracking { get; private set; } = true;

        protected BaseSpecification() { }

        protected BaseSpecification(Expression<Func<T, bool>> criteria)
        {
            Criteria = criteria;
        }

        protected void AddInclude(Expression<Func<T, object>> includeExpression)
        {
            Includes.Add(includeExpression);
        }

        protected void AddInclude(string includeString)
        {
            IncludeStrings.Add(includeString);
        }

        protected void ApplyPaging(int skip, int take)
        {
            Skip = skip;
            Take = take;
            IsPagingEnabled = true;
        }

        protected void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
        {
            OrderBy = orderByExpression;
        }

        protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescExpression)
        {
            OrderByDescending = orderByDescExpression;
        }

        protected void ApplyGroupBy(Expression<Func<T, object>> groupByExpression)
        {
            GroupBy = groupByExpression;
        }

        protected void ApplyDistinct()
        {
            IsDistinct = true;
        }

        protected void ApplyNoTracking()
        {
            AsNoTracking = true;
        }
    }
}

