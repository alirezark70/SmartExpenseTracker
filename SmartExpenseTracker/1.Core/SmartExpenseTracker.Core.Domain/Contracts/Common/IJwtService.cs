using SmartExpenseTracker.Core.Domain.DomainModels.Users.Entities;
using SmartExpenseTracker.Core.Domain.Enums.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.Domain.Contracts.Common
{
    public interface IJwtService
    {
        string GenerateAccessToken(User user, IEnumerable<RoleType> roles, IEnumerable<PermissionType> permissions);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
