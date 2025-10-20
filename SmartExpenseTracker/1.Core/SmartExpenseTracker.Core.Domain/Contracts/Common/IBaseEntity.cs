using SmartExpenseTracker.Core.Domain.DomainModels.Common;
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
        Guid Id { get;}
        DateTime CreatedAt { get;}
        Guid? CreatedBy { get;}
        DateTime? ModifiedAt { get;}
        Guid? ModifiedBy { get;}
        bool IsDeleted { get;}

        IReadOnlyList<IDomainEvent> DomainEvents { get; }

        void AddDomainEvent(IDomainEvent domainEvent);
        void RemoveDomainEvent(IDomainEvent domainEvent);
        void ClearDomainEvents();

    }
}
