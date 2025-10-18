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
        private DateTime _createdAt;
        private Guid? _createdBy;
        private DateTime? _modifiedAt;
        private Guid? _modifiedBy;
        private bool _isDeleted;

        private readonly List<IDomainEvent> _domainEvents = new();
        public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        public Guid Id { get; private set; }

        public DateTime CreatedAt => _createdAt;

        public Guid? CreatedBy => _createdBy;

        public DateTime? ModifiedAt => _modifiedAt;

        public Guid? ModifiedBy => _modifiedBy;

        public bool IsDeleted => _isDeleted;

        protected BaseEntity(Guid id)
        {
            Id = id;
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

        
    }
}
