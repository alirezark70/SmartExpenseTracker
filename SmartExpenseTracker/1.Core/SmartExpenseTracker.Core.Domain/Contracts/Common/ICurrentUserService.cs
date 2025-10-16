using SmartExpenseTracker.Core.Domain.DomainModels.Users;
using SmartExpenseTracker.Core.Domain.Enums.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.Domain.Contracts.Common
{
    public interface ICurrentUserService
    {
        CurrentUser GetCurrentUser(CancellationToken cancellationToken);
        Guid? UserId { get; }

        string? UserName { get; }

        bool IsAuthenticated { get; }

        IReadOnlyList<string> Roles { get; }
        IReadOnlyList<string> Permissions { get; }

        bool IsInRole(string roleName);

        bool HasPermission(string permissionName);
    }
}
