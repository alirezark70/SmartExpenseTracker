using SmartExpenseTracker.Core.Domain.DomainModels.Common;
using SmartExpenseTracker.Core.Domain.Enums.Expense;
using SmartExpenseTracker.Core.Domain.Events.Expense;
using SmartExpenseTracker.Core.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.Domain.DomainModels.Expenses
{
    public class Expense : BaseEntity
    {
        private  Guid _id;
        private readonly DateTime _createdAt;
        public Expense(Guid id, DateTime createdAt) : base(id, createdAt)
        {
            _id = id;
            _createdAt = createdAt;
        }
        public Expense(
            Guid id,
            DateTime createdAt,
        Money amount,
        string description,
        ExpenseCategory category,
        DateTime expenseDate,
        Guid userId,
        PaymentMethod paymentMethod) : this(id,createdAt)
        {
            SetAmount(amount);
            SetDescription(description);
            _category = category;
            _expenseDate = expenseDate;
            _userId = userId;
            _paymentMethod = paymentMethod;
            _id=id;
            _createdAt= createdAt;
        }
        public void UpdateAmount(Money newAmount)
        {
            SetAmount(newAmount);
            SetUpdateTime(_createdAt);
            AddDomainEvent(new ExpenseUpdatedEvent(Id, newAmount));
        }

        public void UpdateDescription(string description)
        {
            SetDescription(description);
            SetUpdateTime(_createdAt);
        }

        public void UpdateCategory(ExpenseCategory category)
        {
            _category = category;
            SetUpdateTime(_createdAt);
        }
        public void AddTag(Tag tag)
        {
            if (!_tags.Contains(tag))
            {
                _tags.Add(tag);
                SetUpdateTime(_createdAt);
            }
        }

        public void RemoveTag(Tag tag)
        {
            if (_tags.Remove(tag))
            {
                SetUpdateTime(_createdAt);
            }
        }

        public void SetReceipt(string receiptUrl)
        {
            if (string.IsNullOrWhiteSpace(receiptUrl))
                throw new ArgumentException("Receipt URL cannot be empty");

            _receiptUrl = receiptUrl;
            SetUpdateTime(_createdAt);
        }

        public void SetLocation(Location location)
        {
            _location = location ?? throw new ArgumentNullException(nameof(location));
            SetUpdateTime(_createdAt);
        }

        public void MakeRecurring(RecurrencePattern pattern)
        {
            _recurrencePattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
            _isRecurring = true;
            SetUpdateTime(_createdAt);
        }

        public void StopRecurring()
        {
            _isRecurring = false;
            _recurrencePattern = null;
            SetUpdateTime(_createdAt);
        }

        private void SetAmount(Money amount)
        {
            _amount = amount ?? throw new ArgumentNullException(nameof(amount));

            if (amount.Value <= 0)
                throw new ArgumentException("Expense amount must be greater than zero");
        }

        private void SetDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Description cannot be empty");

            if (description.Length > 500)
                throw new ArgumentException("Description cannot exceed 500 characters");

            _description = description;
        }
        private Money _amount;
        private string _description;
        private ExpenseCategory _category;
        private DateTime _expenseDate;
        private List<Tag> _tags;
        private Guid _userId;
        private PaymentMethod _paymentMethod;
        private string? _receiptUrl;
        private Location? _location;
        private bool _isRecurring;
        private RecurrencePattern? _recurrencePattern;

        public Money Amount => _amount;
        public string Description => _description;
        public ExpenseCategory Category => _category;
        public DateTime ExpenseDate => _expenseDate;
        public IReadOnlyList<Tag> Tags => _tags.AsReadOnly();
        public Guid UserId => _userId;
        public PaymentMethod PaymentMethod => _paymentMethod;
        public string? ReceiptUrl => _receiptUrl;
        public Location? Location => _location;
        public bool IsRecurring => _isRecurring;
        public RecurrencePattern? RecurrencePattern => _recurrencePattern;
    }
}
