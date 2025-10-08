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
using SmartExpenseTracker.Core.Domain.DomainModels.Users.Entities;
using SmartExpenseTracker.Core.Domain.Enums.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.ApplicationService.CommandHandlers.Identity
{
    public class RegisterCommandHandler : ICommandHandler<RegisterCommand, ApiResponse<AuthResponseDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IJwtSettings _jwtSettings;
        private readonly IIdGenerator<Guid> _idGenerator;
        private readonly IDateTimeProvider _dateTimeProvider;
        public RegisterCommandHandler(
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService,
        IOptions<IJwtSettings> jwtSettings,
        IIdGenerator<Guid> idGenerator,
        IDateTimeProvider dateTimeProvider)
        {
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
            _jwtSettings = jwtSettings.Value;
            _idGenerator = idGenerator;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<ApiResponse<AuthResponseDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            // بررسی وجود کاربر
            var existingUser = await _userManager.FindByNameAsync(request.Request.UserName);
            if (existingUser != null)
                return ApiResponse<AuthResponseDto>.Failure("نام کاربری قبلاً ثبت شده است");

            var existingEmail = await _userManager.FindByEmailAsync(request.Request.Email);
            if (existingEmail != null)
                return ApiResponse<AuthResponseDto>.Failure("ایمیل قبلاً ثبت شده است");

            // ایجاد کاربر جدید
            var user = new ApplicationUser
            {
                Id = _idGenerator.GetId(),
                UserName = request.Request.UserName,
                Email = request.Request.Email,
                FirstName = request.Request.FirstName,
                LastName = request.Request.LastName,
                PhoneNumber = request.Request.PhoneNumber,
                EmailConfirmed = false, // برای پیاده‌سازی تایید ایمیل
                IsActive = true,
                CreatedAt = _dateTimeProvider.GetDateTimeUtcNow(),
            };

            var createResult = await _userManager.CreateAsync(user, request.Request.Password);

            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                return ApiResponse<AuthResponseDto>.Failure($"خطا در ثبت‌نام: {errors}");
            }

            // اختصاص نقش پیش‌فرض
            await _userManager.AddToRoleAsync(user, RoleType.User.ToString());

            // تولید توکن
            var roles = await _userManager.GetRolesAsync(user);

            var rolesEnum = roles
                .Select(r => Enum.TryParse<RoleType>(r, out var role) ? role : (RoleType?)null)
                .Where(r => r.HasValue)
                .Select(r => r.Value)
                .ToList();
            var accessToken = await _jwtTokenService.GenerateAccessTokenAsync(user, rolesEnum);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            // ذخیره Refresh Token
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
            await _userManager.UpdateAsync(user);

            var response = new AuthResponseDto
            {
                UserId = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                Roles = roles.ToList()
            };

            return ApiResponse<AuthResponseDto>.Success(response);
        }
    }
}
