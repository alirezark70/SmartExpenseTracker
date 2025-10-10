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

namespace SmartExpenseTracker.Core.ApplicationService.CommandHandlers.Identity
{
    public class LoginCommandHandler : ICommandHandler<LoginCommand, ApiResponse<AuthResponseDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IJwtSettings _jwtSettings;
        private readonly IDateTimeProvider _dateTimeProvider;
        public LoginCommandHandler(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtTokenService jwtTokenService,
            IOptions<IJwtSettings> jwtSettings,
            IDateTimeProvider dateTimeProvider)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenService = jwtTokenService;
            _jwtSettings = jwtSettings.Value;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<ApiResponse<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            // یافتن کاربر با نام کاربری یا ایمیل
            var user = await _userManager.FindByNameAsync(request.Request.UserNameOrEmail)
                       ?? await _userManager.FindByEmailAsync(request.Request.UserNameOrEmail);

            if (user == null)
                return ApiResponse<AuthResponseDto>.Failure("نام کاربری یا رمز عبور اشتباه است");

            if (!user.IsActive)
                return ApiResponse<AuthResponseDto>.Failure("حساب کاربری غیرفعال است");

            // بررسی رمز عبور
            var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Request.Password, lockoutOnFailure: true);

            if (signInResult.IsLockedOut)
                return ApiResponse<AuthResponseDto>.Failure("حساب شما به دلیل تلاش‌های ناموفق زیاد قفل شده است");

            if (!signInResult.Succeeded)
                return ApiResponse<AuthResponseDto>.Failure("نام کاربری یا رمز عبور اشتباه است");

            // تولید توکن‌ها
            var roles = await _userManager.GetRolesAsync(user);
            var rolesEnum = roles
                .Select(r => Enum.TryParse<RoleType>(r, out var role) ? role : (RoleType?)null)
                .Where(r => r.HasValue)
                .Select(r => r.Value)
                .ToList();

            var accessToken = await _jwtTokenService.GenerateAccessTokenAsync(user, rolesEnum);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            // به‌روزرسانی اطلاعات ورود
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = _dateTimeProvider.GetDateTimeUtcNow().AddDays(_jwtSettings.RefreshTokenExpirationDays);
            user.LastLoginAt = _dateTimeProvider.GetDateTimeUtcNow();
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
                ExpiresAt = _dateTimeProvider.GetDateTimeUtcNow().AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),  
                Roles = roles.ToList()
            };

            return ApiResponse<AuthResponseDto>.Success(response);
        }
    }
}
