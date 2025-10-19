using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SmartExpenseTracker.Core.ApplicationService.Commands.Identity;
using SmartExpenseTracker.Core.ApplicationService.Contracts;
using SmartExpenseTracker.Core.ApplicationService.Contracts.Mediator;
using SmartExpenseTracker.Core.ApplicationService.Contracts.Persistence;
using SmartExpenseTracker.Core.ApplicationService.Contracts.Persistence.Users;
using SmartExpenseTracker.Core.ApplicationService.Dtos.Identity;
using SmartExpenseTracker.Core.Domain.Contracts.Common;
using SmartExpenseTracker.Core.Domain.DomainModels.Identity;
using SmartExpenseTracker.Core.Domain.DomainModels.Response.Entities;
using SmartExpenseTracker.Core.Domain.Enums.Users;

namespace SmartExpenseTracker.Core.ApplicationService.CommandHandlers.Identity
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, ApiResponse<AuthResponseDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IJwtSettings _jwtSettings;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IUserRepository _userRepository;
        private readonly IGuidIdGenerator _guidIdGenerator;

        public LoginCommandHandler(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtTokenService jwtTokenService,
            IOptions<IJwtSettings> jwtSettings,
            IDateTimeProvider dateTimeProvider,
            IUserRepository userRepository,
            IGuidIdGenerator guidIdGenerator)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenService = jwtTokenService;
            _jwtSettings = jwtSettings.Value;
            _dateTimeProvider = dateTimeProvider;
            _userRepository = userRepository;
            _guidIdGenerator = guidIdGenerator;
        }

        public async Task<ApiResponse<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            // Find user by username or email
            var user = await _userManager.FindByNameAsync(request.Request.UserNameOrEmail)
                       ?? await _userManager.FindByEmailAsync(request.Request.UserNameOrEmail);

            if (user == null)
            {
                // Log failed login attempt (without revealing that user doesn't exist)
                await LogFailedLoginAttempt(null, request.Request.UserNameOrEmail, "User not found");
                return ApiResponse<AuthResponseDto>.Failure("نام کاربری یا رمز عبور اشتباه است");
            }

            if (!user.IsActive)
            {
                await LogFailedLoginAttempt(user.Id, request.Request.UserNameOrEmail, "Account inactive");
                return ApiResponse<AuthResponseDto>.Failure("حساب کاربری غیرفعال است");
            }

            // Check password
            var signInResult = await _signInManager.CheckPasswordSignInAsync(
                user,
                request.Request.Password,
                lockoutOnFailure: true);

            if (signInResult.IsLockedOut)
            {
                await LogFailedLoginAttempt(user.Id, request.Request.UserNameOrEmail, "Account locked");
                return ApiResponse<AuthResponseDto>.Failure("حساب شما به دلیل تلاش‌های ناموفق زیاد قفل شده است");
            }

            if (!signInResult.Succeeded)
            {
                await LogFailedLoginAttempt(user.Id, request.Request.UserNameOrEmail, "Invalid password");
                user.RecordFailedLogin(_dateTimeProvider.GetDateTimeUtcNow());
                await _userManager.UpdateAsync(user);
                return ApiResponse<AuthResponseDto>.Failure("نام کاربری یا رمز عبور اشتباه است");
            }

            // Generate tokens
            var roles = await _userManager.GetRolesAsync(user);
            var rolesEnum = roles
                .Select(r => Enum.TryParse<RoleType>(r, out var role) ? role : (RoleType?)null)
                .Where(r => r.HasValue)
                .Select(r => r.Value)
                .ToList();

            var accessToken = await _jwtTokenService.GenerateAccessTokenAsync(user, rolesEnum);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            // Update login information
            var ipAddress = GetIpAddress();
            user.SetRefreshToken(_dateTimeProvider.GetDateTimeUtcNow(), refreshToken, _jwtSettings.RefreshTokenExpirationDays);
            user.RecordLogin(_dateTimeProvider.GetDateTimeUtcNow(), ipAddress);
            await _userManager.UpdateAsync(user);

            // Log successful login
            await LogSuccessfulLogin(user.Id, ipAddress);

            // Log user activity
            await LogUserActivity(
                user.Id,
                "Login",
                $"User logged in from IP: {ipAddress}",
                ipAddress);

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

        private async Task LogSuccessfulLogin(Guid userId, string? ipAddress)
        {
            var loginHistory = new UserLoginHistory(
                _guidIdGenerator.GetId(),
                userId,
                ipAddress ?? "Unknown",
                GetUserAgent(),
                true);

            await _userRepository.AddLoginHistoryAsync(loginHistory);
        }

        private async Task LogFailedLoginAttempt(Guid? userId, string attemptedUserName, string failureReason)
        {
            if (userId.HasValue)
            {
                var loginHistory = new UserLoginHistory(
                    _guidIdGenerator.GetId(),
                    userId.Value,
                    GetIpAddress() ?? "Unknown",
                    GetUserAgent(),
                    false,
                    failureReason);

                await _userRepository.AddLoginHistoryAsync(loginHistory);
            }
        }

        private async Task LogUserActivity(Guid userId, string activityType, string description, string? ipAddress)
        {
            var activity = new UserActivity(
                _guidIdGenerator.GetId(),
                userId,
                activityType,
                description,
                ipAddress,
                GetActivityMetadata());

            await _userRepository.AddUserActivityAsync(activity);
        }

        private string? GetIpAddress()
        {
            // This should be injected from HttpContext
            // For now, returning placeholder
            return "127.0.0.1";
        }

        private string? GetUserAgent()
        {
            // This should be injected from HttpContext
            // For now, returning placeholder
            return "Mozilla/5.0";
        }

        private string GetActivityMetadata()
        {
            return System.Text.Json.JsonSerializer.Serialize(new
            {
                Browser = "Chrome",
                OS = "Windows",
                Device = "Desktop"
            });
        }
    }
}
