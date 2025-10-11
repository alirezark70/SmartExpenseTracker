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

        private readonly List<IDomainEvent> _domainEvents = new();
        public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
        public string? Description { get;private set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? CreatedBy { get;private set; }
        public DateTime? ModifiedAt { get; private set; }
        public string? ModifiedBy { get; private set; }
        public bool IsDeleted { get; private set; }

        public void AddNewRole(Guid id,DateTime createAt,string createBy,string description)
        {
            Id=id;
            CreatedAt=createAt;
            CreatedBy=createBy;
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
