using SmartExpenseTracker.Core.Domain.DomainModels.Common;

namespace SmartExpenseTracker.Core.Domain.DomainModels.Users.Entities
{
    public sealed class Permission: BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Resource { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;

        // رابطه با نقش‌ها
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
