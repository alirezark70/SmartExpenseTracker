using SmartExpenseTracker.Core.Domain.Events.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.Domain.Contracts.Common
{
    public interface IBaseEntity
    {
        Guid Id { get;  }
        DateTime CreatedAt { get; set; }
        string? CreatedBy { get; set; }
        DateTime? ModifiedAt { get; set; }
        string? ModifiedBy { get;  }
        bool IsDeleted { get; }
        IReadOnlyList<IDomainEvent> DomainEvents { get; }

        void AddDomainEvent(IDomainEvent domainEvent);
        void RemoveDomainEvent(IDomainEvent domainEvent);
        void ClearDomainEvents();
        void MarkAsDeleted(DateTime modifiedAt);
    }
}
