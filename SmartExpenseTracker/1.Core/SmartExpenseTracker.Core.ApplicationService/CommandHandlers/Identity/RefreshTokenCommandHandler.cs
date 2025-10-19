using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SmartExpenseTracker.Core.ApplicationService.Commands.Identity;
using SmartExpenseTracker.Core.ApplicationService.Contracts;
using SmartExpenseTracker.Core.ApplicationService.Contracts.Mediator;
using SmartExpenseTracker.Core.ApplicationService.Contracts.Persistence;
using SmartExpenseTracker.Core.ApplicationService.Dtos.Identity;
using SmartExpenseTracker.Core.Domain.Contracts.Common;
using SmartExpenseTracker.Core.Domain.DomainModels.Identity;
using SmartExpenseTracker.Core.Domain.DomainModels.Response.Entities;
using SmartExpenseTracker.Core.Domain.Enums.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.ApplicationService.CommandHandlers.Identity
{
    public class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, ApiResponse<AuthResponseDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IJwtSettings _jwtSettings;
        private readonly IDateTimeProvider _dateTimeProvider;

        public RefreshTokenCommandHandler(
            UserManager<ApplicationUser> userManager,
            IJwtTokenService jwtTokenService,
            IOptions<IJwtSettings> jwtSettings,
            IDateTimeProvider dateTimeProvider)
        {
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
            _jwtSettings = jwtSettings.Value;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<ApiResponse<AuthResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            // بررسی توکن منقضی شده
            var principal = _jwtTokenService.GetPrincipalFromExpiredToken(request.Request.AccessToken);
            if (principal == null)
            {
                return ApiResponse<AuthResponseDto>.Failure("توکن نامعتبر است", Domain.Enums.Response.ResponseStatus.Unauthorized);
            }

            var userId = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return ApiResponse<AuthResponseDto>.Failure("توکن نامعتبر است", Domain.Enums.Response.ResponseStatus.Unauthorized);
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !user.IsActive || user.IsDeleted)
            {
                return ApiResponse<AuthResponseDto>.Failure("کاربر یافت نشد یا غیرفعال است", Domain.Enums.Response.ResponseStatus.Unauthorized);
            }

            // اعتبارسنجی Refresh Token
            if (!ValidateRefreshToken(user, request.Request.RefreshToken))
            {
                return ApiResponse<AuthResponseDto>.Failure("توکن تازه‌سازی نامعتبر است", Domain.Enums.Response.ResponseStatus.Unauthorized);
            }

            // بررسی انقضای Refresh Token
            if (user.RefreshTokenExpiryTime <= _dateTimeProvider.GetDateTimeUtcNow())
            {
                return ApiResponse<AuthResponseDto>.Failure("توکن تازه‌سازی منقضی شده است", Domain.Enums.Response.ResponseStatus.Unauthorized);
            }

            // دریافت نقش‌ها
            var roles = await _userManager.GetRolesAsync(user);
            var rolesEnum = roles
                .Select(r => Enum.TryParse<RoleType>(r, out var role) ? role : (RoleType?)null)
                .Where(r => r.HasValue)
                .Select(r => r.Value)
                .ToList();

            // تولید توکن‌های جدید
            var newAccessToken = await _jwtTokenService.GenerateAccessTokenAsync(user, rolesEnum);
            var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

            // به‌روزرسانی Refresh Token
            user.SetRefreshToken(_dateTimeProvider.GetDateTimeUtcNow(), newRefreshToken, _jwtSettings.RefreshTokenExpirationDays);
            await _userManager.UpdateAsync(user);

            var response = new AuthResponseDto
            {
                UserId = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = _dateTimeProvider.GetDateTimeUtcNow().AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                Roles = roles.ToList()
            };

            return ApiResponse<AuthResponseDto>.Success(response);
        }

        private bool ValidateRefreshToken(ApplicationUser user, string refreshToken)
        {
            if (string.IsNullOrEmpty(user.RefreshTokenHash) || string.IsNullOrEmpty(user.RefreshTokenSalt))
                return false;

            try
            {
                var salt = Convert.FromBase64String(user.RefreshTokenSalt);
                using var pbkdf2 = new Rfc2898DeriveBytes(
                    refreshToken,
                    salt,
                    10000,
                    HashAlgorithmName.SHA256);
                var computedHash = Convert.ToBase64String(pbkdf2.GetBytes(32));
                return computedHash == user.RefreshTokenHash;
            }
            catch
            {
                return false;
            }
        }
    }
}
