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
        public DateTime CreatedAt { get; } 

        public Guid? CreatedBy { get; }
        public DateTime? ModifiedAt { get; private set; }
        public Guid? ModifiedBy { get; private set; }
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
        public void RemoveDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Remove(domainEvent);

        }

        public void SoftDelete(DateTime modifiedAt, Guid? modifiedBy)
        {
            IsDeleted = true;
            Touch(modifiedAt, modifiedBy);
        }

        public void Touch(DateTime modifieddAt, Guid? modifieddBy)
        {
            ModifiedAt = modifieddAt;
            ModifiedBy = modifieddBy;
        }
    }
}
