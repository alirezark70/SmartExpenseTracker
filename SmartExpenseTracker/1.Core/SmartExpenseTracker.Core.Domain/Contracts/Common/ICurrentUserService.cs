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
        string? UserId { get; }
        string? Username { get; }
        bool IsAuthenticated { get; }
        IEnumerable<RoleType> Roles { get; }
        IEnumerable<PermissionType> Permissions { get; }
    }
}
