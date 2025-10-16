using Microsoft.AspNetCore.Http;
using SmartExpenseTracker.Core.ApplicationService.Contracts.Base;
using SmartExpenseTracker.Core.ApplicationService.Extensions.Helper;
using SmartExpenseTracker.Core.Domain.DomainModels.Users;
using SmartExpenseTracker.Core.Domain.Enums.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Infra.Persistence.Services.Base
{
    public class CustomContextAccessor : ICustomContextAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private HttpContext? _httpContext => _httpContextAccessor.HttpContext;

        public CustomContextAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? UserId => Guid.TryParse( _httpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid userIdGuid)?userIdGuid:Guid.Empty;

        public string? UserName => _httpContext?.User?.Identity?.Name;

        public string? UserEmail => _httpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

        public string? IpAddress => _httpContext?.Connection?.RemoteIpAddress?.ToString();

        public string? UserAgent => _httpContext?.Request?.Headers["User-Agent"].ToString();

        public string? TenantId => _httpContext?.User?.FindFirst("TenantId")?.Value;

        public bool IsAuthenticated => _httpContext?.User?.Identity?.IsAuthenticated ?? false;

        public IReadOnlyList<RoleType> Roles => _httpContext?.User?.Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value.ToEnumOrDefault<RoleType>())
                    .Where(r => r.HasValue)
                    .Select(r => r.Value).ToList().AsReadOnly()
                    ?? new List<RoleType>().AsReadOnly();

        public IReadOnlyList<string> Permissions => _httpContextAccessor.HttpContext?.User?
            .FindAll("permission")
            .Select(c => c.Value)
            .ToList() ?? new List<string>();

        public CurrentUser CurrentUser
        {
            get
            {
                var email = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);
                return new CurrentUser
                {
                    UserId = UserId != null && UserId != default ? UserId.Value : Guid.Empty,
                    UserName = UserName ?? "",
                    Email = email!,
                    Roles = Roles
                };
            }
        }
            
        public T? GetClaimValue<T>(string claimType)
        {
            var claimValue = _httpContext?.User?.FindFirst(claimType)?.Value;
            if (string.IsNullOrEmpty(claimValue))
                return default;

            try
            {
                return (T)Convert.ChangeType(claimValue, typeof(T));
            }
            catch
            {
                return default;
            }
        }

        public string? GetCustomHeader(string key)
        {
            return _httpContext?.Request.Headers[key].ToString();
        }

        public void SetCustomHeader(string key, string value)
        {
            _httpContext?.Response.Headers.Add(key, value);
        }

        public bool IsInRole(string roleName) => _httpContextAccessor.HttpContext?.User?.IsInRole(roleName) ?? false;

        public bool HasPermission(string permissionName) => Permissions.Contains(permissionName);
    }
}
