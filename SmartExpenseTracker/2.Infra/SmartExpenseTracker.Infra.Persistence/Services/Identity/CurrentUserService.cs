using Microsoft.AspNetCore.Http;
using SmartExpenseTracker.Core.ApplicationService.DataTransfareObject.Users;
using SmartExpenseTracker.Core.Domain.Contracts.Common;
using SmartExpenseTracker.Core.Domain.DomainModels.Users;
using SmartExpenseTracker.Infra.Mapping.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Infra.Persistence.Services.Identity
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;

        }

        //public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        public Guid? UserId => Guid.TryParse(_httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userIdGuid) ? userIdGuid : Guid.Empty;

        public string? UserName => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);

        public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

        public IReadOnlyList<string> Roles => _httpContextAccessor.HttpContext?.User?
            .FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList() ?? new List<string>();

        public IReadOnlyList<string> Permissions => _httpContextAccessor.HttpContext?.User?
            .FindAll("permission")
            .Select(c => c.Value)
            .ToList() ?? new List<string>();

        public CurrentUser GetCurrentUser(CancellationToken cancellationToken)
        {
            var email = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);

            return new CurrentUser
            {
                UserId = UserId != null && UserId != default ? UserId.Value : Guid.Empty,
                UserName = UserName ?? "",
                Email = email!,
                Roles = Roles.ToList() ?? new List<string>()
            };
        }

        public bool HasPermission(string permissionName) => Permissions.Contains(permissionName);

        public bool IsInRole(string roleName) => _httpContextAccessor.HttpContext?.User?.IsInRole(roleName) ?? false;
    }
}
