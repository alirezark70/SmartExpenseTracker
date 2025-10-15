using Microsoft.AspNetCore.Http;
using SmartExpenseTracker.Core.Domain.Contracts.Common;
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

        public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

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

        public bool HasPermission(string permissionName) =>  Permissions.Contains(permissionName);

        public bool IsInRole(string roleName)=> _httpContextAccessor.HttpContext?.User?.IsInRole(roleName) ?? false;
    }
}
