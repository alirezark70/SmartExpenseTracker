using SmartExpenseTracker.Core.Domain.DomainModels.Common;
using SmartExpenseTracker.Core.Domain.Enums.Expense;
using SmartExpenseTracker.Core.Domain.Events.Budget;
using SmartExpenseTracker.Core.Domain.ValueObjects;

namespace SmartExpenseTracker.Core.Domain.DomainModels.Budgets
{
    public class Budget : BaseEntity
    {
        public Budget(Guid id, DateTime createdAt) : base(id, createdAt)
        {
            _alerts = new List<BudgetAlert>();
            _spent = Money.Zero("USD");
        }

        private string _name;
        private Money _limit;
        private Money _spent;
        private ExpenseCategory _category;
        private DateRange _period;
        private Guid _userId;
        private bool _isActive;
        private List<BudgetAlert> _alerts;

        public string Name => _name;
        public Money Limit => _limit;
        public Money Spent => _spent;
        public Money Remaining => new Money(Limit.Value - Spent.Value, Limit.Currency);
        public decimal UsagePercentage => Limit.Value > 0 ? (Spent.Value / Limit.Value) * 100 : 0;
        public ExpenseCategory Category => _category;
        public DateRange Period => _period;
        public Guid UserId => _userId;
        public bool IsActive => _isActive;
        public IReadOnlyList<BudgetAlert> Alerts => _alerts.AsReadOnly();


        public Budget(
            Guid id,
            DateTime createdAt,
            string name,
            Money limit,
            ExpenseCategory category,
            DateRange period,
            Guid userId) : this(id,createdAt)
        {
            SetName(name);
            SetLimit(limit);
            _category = category;
            _period = period ?? throw new ArgumentNullException(nameof(period));
            _userId = userId;
            _isActive = true;
        }

        public void AddExpense(Money amount, Guid budgetThresholdReachedEventId, DateTime occurredOn)
        {
            if (amount.Currency != _limit.Currency)
                throw new InvalidOperationException($"Currency mismatch. Expected {_limit.Currency}, got {amount.Currency}");

            _spent = new Money(_spent.Value + amount.Value, _spent.Currency);

            CheckBudgetAlerts(budgetThresholdReachedEventId, occurredOn);
            SetUpdateTime(CreatedAt);
        }

        public void ResetSpent()
        {
            _spent = Money.Zero(_limit.Currency);
            SetUpdateTime(CreatedAt);
        }

        public void UpdateLimit(Money newLimit, Guid budgetThresholdReachedEventId, DateTime occurredOn)
        {
            SetLimit(newLimit);
            CheckBudgetAlerts(budgetThresholdReachedEventId, occurredOn);
            SetUpdateTime(CreatedAt);
        }

        public void Deactivate()
        {
            _isActive = false;
            SetUpdateTime(CreatedAt);
        }

        public void Activate()
        {
            _isActive = true;
            SetUpdateTime(CreatedAt);
        }

        public void AddAlert(decimal thresholdPercentage,Guid budgetAlertId,DateTime createdAt)
        {
            if (thresholdPercentage <= 0 || thresholdPercentage > 100)
                throw new ArgumentException("Threshold percentage must be between 0 and 100");

            var alert = new BudgetAlert(Id, thresholdPercentage, budgetAlertId, createdAt);

            if (!_alerts.Any(a => a.ThresholdPercentage == thresholdPercentage))
            {
                _alerts.Add(alert);
                SetUpdateTime(CreatedAt);
            }
        }

        private void CheckBudgetAlerts(Guid budgetThresholdReachedEventId, DateTime occurredOn)
        {
            foreach (var alert in _alerts.Where(a => !a.IsTriggered))
            {
                if (UsagePercentage >= alert.ThresholdPercentage)
                {
                    alert.Trigger();
                    AddDomainEvent(new BudgetThresholdReachedEvent(budgetThresholdReachedEventId, occurredOn, Id, UsagePercentage, alert.ThresholdPercentage));
                }
            }
        }

        private void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Budget name cannot be empty");

            if (name.Length > 100)
                throw new ArgumentException("Budget name cannot exceed 100 characters");

            _name = name;
        }

        private void SetLimit(Money limit)
        {
            _limit = limit ?? throw new ArgumentNullException(nameof(limit));

            if (limit.Value <= 0)
                throw new ArgumentException("Budget limit must be greater than zero");
        }
    }
}
