using SmartExpenseTracker.Core.Domain.Contracts.Common;
using SmartExpenseTracker.Core.Domain.Events.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.Domain.DomainModels.Common
{
    public abstract class BaseEntity : IBaseEntity
    {
        private readonly List<IDomainEvent> _domainEvents = new();
        public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        public Guid Id { get; protected set; }

        public DateTime CreatedAt { get; protected set; }

        public string? CreatedBy { get; protected set; }

        public DateTime? ModifiedAt { get; protected set; }

        public string? ModifiedBy { get; protected set; }

        public bool IsDeleted { get; protected set; }

        protected BaseEntity(Guid id, DateTime createdAt)
        {
            Id = id;
            CreatedAt = createdAt;
        }


        //برای اینکه در 
        //ef core
        //نیاز به سازنده بدون پارامتر پیش فرض داره 
        protected BaseEntity()
        {
            _domainEvents = new List<IDomainEvent>();
        }

        public void AddDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public void RemoveDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Remove(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }

        public void MarkAsDeleted(DateTime modifiedAt)
        {
            IsDeleted = true;
            ModifiedAt = modifiedAt;
        }

        protected void SetUpdateTime(DateTime modifiedAt)
        {
            ModifiedAt = modifiedAt;
        }

    }
}
