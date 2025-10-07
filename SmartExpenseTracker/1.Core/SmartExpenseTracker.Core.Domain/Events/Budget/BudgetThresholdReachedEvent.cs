using SmartExpenseTracker.Core.Domain.Events.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.Domain.Events.Budget
{
    public class BudgetThresholdReachedEvent : IDomainEvent
    {
        public Guid Id { get; }
        public DateTime OccurredOn { get; }
        public Guid BudgetId { get; }
        public decimal CurrentUsagePercentage { get; }
        public decimal ThresholdPercentage { get; }

        public BudgetThresholdReachedEvent(Guid id,DateTime occurredOn, Guid budgetId, decimal currentUsagePercentage, decimal thresholdPercentage)
        {
            Id = id;
            OccurredOn = occurredOn;
            BudgetId = budgetId;
            CurrentUsagePercentage = currentUsagePercentage;
            ThresholdPercentage = thresholdPercentage;
        }
    }
}
