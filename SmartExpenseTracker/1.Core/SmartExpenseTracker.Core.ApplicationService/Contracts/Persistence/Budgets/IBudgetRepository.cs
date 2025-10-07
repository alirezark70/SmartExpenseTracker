using SmartExpenseTracker.Core.Domain.DomainModels.Budgets;
using SmartExpenseTracker.Core.Domain.Enums.Expense;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.ApplicationService.Contracts.Persistence.Budgets
{
    public interface IBudgetRepository : IRepository<Budget>
    {
        Task<IEnumerable<Budget>> GetActiveBudgetsAsync(
            Guid userId,
            CancellationToken cancellationToken = default);

        Task<Budget?> GetBudgetByCategoryAsync(
            Guid userId,
            ExpenseCategory category,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<Budget>> GetExceededBudgetsAsync(
            Guid userId,
            CancellationToken cancellationToken = default);
    }
}
