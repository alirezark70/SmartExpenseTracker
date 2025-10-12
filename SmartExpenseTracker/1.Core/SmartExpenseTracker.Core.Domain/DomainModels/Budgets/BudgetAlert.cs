using SmartExpenseTracker.Core.Domain.DomainModels.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.Domain.DomainModels.Budgets
{
    public class BudgetAlert : BaseEntity
    {
        public Guid BudgetId { get; private set; }
        public decimal ThresholdPercentage { get; private set; }
        public bool IsTriggered { get; private set; }
        public DateTime? TriggeredAt { get; private set; }

        public BudgetAlert(Guid budgetId, decimal thresholdPercentage,Guid id) :base(id)
        {
            BudgetId = budgetId;
            ThresholdPercentage = thresholdPercentage;
            IsTriggered = false;
        }

        public void Trigger(DateTime triggerdAt)
        {
            IsTriggered = true;
            TriggeredAt = triggerdAt;
        }

        public void Reset()
        {
            IsTriggered = false;
            TriggeredAt = null;
        }
    }
}