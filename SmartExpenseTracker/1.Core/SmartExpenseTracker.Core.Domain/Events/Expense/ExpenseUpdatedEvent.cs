using SmartExpenseTracker.Core.Domain.Events.Base;
using SmartExpenseTracker.Core.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.Domain.Events.Expense
{
    public class ExpenseUpdatedEvent : IDomainEvent
    {
        public Guid Id { get; }
        public DateTime OccurredOn { get; }
        public Guid ExpenseId { get; }
        public Money NewAmount { get; }

        public ExpenseUpdatedEvent(Guid expenseId, Money newAmount)
        {
            Id = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
            ExpenseId = expenseId;
            NewAmount = newAmount;
        }
    }
}
