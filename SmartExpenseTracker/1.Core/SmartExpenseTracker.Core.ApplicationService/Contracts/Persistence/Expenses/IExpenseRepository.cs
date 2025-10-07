using SmartExpenseTracker.Core.Domain.DomainModels.Expenses;
using SmartExpenseTracker.Core.Domain.DomainModels.Response.Entities;
using SmartExpenseTracker.Core.Domain.Enums.Expense;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.ApplicationService.Contracts.Persistence.Expenses
{
    public interface IExpenseRepository : IRepository<Expense>
    {
        Task<PagedResponse<Expense>> GetPagedExpensesAsync(
            Guid userId,
            int pageNumber,
            int pageSize,
            ExpenseCategory? category = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            CancellationToken cancellationToken = default);

        Task<decimal> GetTotalSpentAsync(
            Guid userId,
            ExpenseCategory? category = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<Expense>> GetRecurringExpensesAsync(
            Guid userId,
            CancellationToken cancellationToken = default);
    }
}
