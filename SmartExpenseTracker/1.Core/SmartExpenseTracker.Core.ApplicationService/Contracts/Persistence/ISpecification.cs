using SmartExpenseTracker.Core.Domain.DomainModels.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.ApplicationService.Contracts.Persistence
{
    public interface ISpecification<T> where T : BaseEntity
    {
        Expression<Func<T, bool>> Criteria { get; }
        List<Expression<Func<T, object>>> Includes { get; }
        List<string> IncludeStrings { get; }
        Expression<Func<T, object>>? OrderBy { get; }
        Expression<Func<T, object>>? OrderByDescending { get; }
        Expression<Func<T, object>>? GroupBy { get; }

        int Take { get; }
        int Skip { get; }
        bool IsPagingEnabled { get; }
        bool IsDistinct { get; }
        bool AsNoTracking { get; }
    }
}
