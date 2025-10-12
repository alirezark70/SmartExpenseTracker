using Microsoft.AspNetCore.Identity;
using SmartExpenseTracker.Core.Domain.Contracts.Common;
using SmartExpenseTracker.Core.Domain.DomainModels.Common;
using SmartExpenseTracker.Core.Domain.Events.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.Domain.DomainModels.Identity
{
    public class ApplicationRole : IdentityRole<Guid>, IBaseEntity
    {
        public ApplicationRole() { } // for EF Core
        public ApplicationRole(string description) { Description = description; } 

        private readonly List<IDomainEvent> _domainEvents = new();
        public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
        public string? Description { get;private set; }
        public DateTime CreatedAt { get; set; } 

        public string? CreatedBy { get; set; }
        public DateTime? ModifiedAt { get;  set; }
        public string? ModifiedBy { get;  set; }
        public bool IsDeleted { get; private set; }

        public void AddNewRole(Guid id,string description)
        {
            Id=id;
            Description = description;
        }

        public ICollection<ApplicationUserRole> UserRoles { get; set; } = new List<ApplicationUserRole>();

        public void AddDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
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

        public void RemoveDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Remove(domainEvent);

        }
    }
}
