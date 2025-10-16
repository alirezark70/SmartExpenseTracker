using SmartExpenseTracker.Core.Domain.DomainModels.Users;
using SmartExpenseTracker.Core.Domain.Enums.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.ApplicationService.Contracts.Base
{
    public interface ICustomContextAccessor
    {
        Guid? UserId { get; }
        string? UserName { get; }
        string? UserEmail { get; }
        string? IpAddress { get; }
        string? UserAgent { get; }
        string? TenantId { get; }
        bool IsAuthenticated { get; }
        IReadOnlyList<RoleType> Roles { get; }
        IReadOnlyList<string> Permissions { get; }
        CurrentUser CurrentUser { get; }
        bool IsInRole(string roleName);
        bool HasPermission(string permissionName);
        T? GetClaimValue<T>(string claimType);
        void SetCustomHeader(string key, string value);
        string? GetCustomHeader(string key);
    }
}
