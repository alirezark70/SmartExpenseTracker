using SmartExpenseTracker.Core.Domain.DomainModels.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.Domain.DomainModels.Users.Entities
{
    public sealed class Role : BaseEntity
    {
        public Role(Guid id, DateTime createdAt) : base(id)
        {
        }

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // رابطه با کاربران
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        // رابطه با دسترسی‌ها
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
