using SmartExpenseTracker.Core.Domain.DomainModels.Common;
using SmartExpenseTracker.Core.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.Domain.DomainModels
{
    public class Expense : BaseEntity
    {
        public Expense(Guid id, DateTime createdAt) : base(id, createdAt)
        {
        }

        //private Money _amount;
        //private string _description;
        //private ExpenseCategory _category;
        //private DateTime _expenseDate;
        //private List<Tag> _tags;
        //private Guid _userId;
        //private PaymentMethod _paymentMethod;
        //private string? _receiptUrl;
        //private Location? _location;
        //private bool _isRecurring;
        //private RecurrencePattern? _recurrencePattern;

        //public Money Amount => _amount;
        //public string Description => _description;
        //public ExpenseCategory Category => _category;
        //public DateTime ExpenseDate => _expenseDate;
        //public IReadOnlyList<Tag> Tags => _tags.AsReadOnly();
        //public Guid UserId => _userId;
        //public PaymentMethod PaymentMethod => _paymentMethod;
        //public string? ReceiptUrl => _receiptUrl;
        //public Location? Location => _location;
        //public bool IsRecurring => _isRecurring;
        //public RecurrencePattern? RecurrencePattern => _recurrencePattern;
    }
}
