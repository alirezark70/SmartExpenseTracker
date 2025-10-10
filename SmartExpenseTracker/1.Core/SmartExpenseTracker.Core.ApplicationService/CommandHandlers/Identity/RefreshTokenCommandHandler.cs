using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SmartExpenseTracker.Core.ApplicationService.Commands.Identity;
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
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.ApplicationService.CommandHandlers.Identity
{
    public class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, ApiResponse<AuthResponseDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IJwtSettings _jwtSettings;

        public RefreshTokenCommandHandler(
            UserManager<ApplicationUser> userManager,
            IJwtTokenService jwtTokenService,
            IOptions<IJwtSettings> jwtSettings)
        {
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<ApiResponse<AuthResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var principal = _jwtTokenService.GetPrincipalFromExpiredToken(request.Request.AccessToken);

            if (principal == null)
                return ApiResponse<AuthResponseDto>.Failure("توکن نامعتبر است");

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return ApiResponse<AuthResponseDto>.Failure("توکن نامعتبر است");

            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null || !user.IsActive)
                return ApiResponse<AuthResponseDto>.Failure("کاربر یافت نشد یا غیرفعال است");

            if (user.RefreshToken != request.Request.RefreshToken)
                return ApiResponse<AuthResponseDto>.Failure("توکن تازه‌سازی نامعتبر است");

            if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return ApiResponse<AuthResponseDto>.Failure("توکن تازه‌سازی منقضی شده است");

            var roles = await _userManager.GetRolesAsync(user);
            var rolesEnum = roles
                .Select(r => Enum.TryParse<RoleType>(r, out var role) ? role : (RoleType?)null)
                .Where(r => r.HasValue)
                .Select(r => r.Value)
                .ToList();


            // تولید توکن‌های جدید
            
            var newAccessToken = await _jwtTokenService.GenerateAccessTokenAsync(user, rolesEnum);
            var newRefreshToken = _jwtTokenService.GenerateRefreshToken();




            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
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
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                Roles = roles.ToList()
            };

            return ApiResponse<AuthResponseDto>.Success(response);
        }
    }
}
