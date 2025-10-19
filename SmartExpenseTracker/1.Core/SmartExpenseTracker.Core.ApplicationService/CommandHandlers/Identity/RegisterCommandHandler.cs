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
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, ApiResponse<AuthResponseDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IJwtSettings _jwtSettings;
        private readonly IGuidIdGenerator _guidIdGenerator;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IUserRepository _userRepository;

        public RegisterCommandHandler(
            UserManager<ApplicationUser> userManager,
            IJwtTokenService jwtTokenService,
            IOptions<IJwtSettings> jwtSettings,
            IDateTimeProvider dateTimeProvider,
            IGuidIdGenerator guidIdGenerator,
            IUserRepository userRepository)
        {
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
            _jwtSettings = jwtSettings.Value;
            _dateTimeProvider = dateTimeProvider;
            _guidIdGenerator = guidIdGenerator;
            _userRepository = userRepository;
        }

        public async Task<ApiResponse<AuthResponseDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            // Check for existing user
            var existingUser = await _userManager.FindByNameAsync(request.Request.UserName);
            if (existingUser != null)
                return ApiResponse<AuthResponseDto>.Failure("نام کاربری قبلاً ثبت شده است");

            var existingEmail = await _userManager.FindByEmailAsync(request.Request.Email);
            if (existingEmail != null)
                return ApiResponse<AuthResponseDto>.Failure("ایمیل قبلاً ثبت شده است");

            // Create new user
            var user = new ApplicationUser(
                _guidIdGenerator.GetId(),
                _dateTimeProvider.GetDateTimeUtcNow(),
                request.Request.UserName,
                request.Request.Email,
                request.Request.FirstName,
                request.Request.LastName,
                request.Request.PhoneNumber);

            var createResult = await _userManager.CreateAsync(user, request.Request.Password);

            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                return ApiResponse<AuthResponseDto>.Failure($"خطا در ثبت ‌نام: {errors}");
            }

            // Assign default role
            await _userManager.AddToRoleAsync(user, RoleType.User.ToString());

            // Generate tokens
            var roles = await _userManager.GetRolesAsync(user);
            var rolesEnum = roles
                .Select(r => Enum.TryParse<RoleType>(r, out var role) ? role : (RoleType?)null)
                .Where(r => r.HasValue)
                .Select(r => r.Value)
                .ToList();

            var accessToken = await _jwtTokenService.GenerateAccessTokenAsync(user, rolesEnum);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            // Save Refresh Token
            user.SetRefreshToken(_dateTimeProvider.GetDateTimeUtcNow(), refreshToken, _jwtSettings.RefreshTokenExpirationDays);
            await _userManager.UpdateAsync(user);

            // Log user activity
            var activity = new UserActivity(
                _guidIdGenerator.GetId(),
                user.Id,
                "Registration",
                "New user registered",
                GetIpAddress(),
                null);

            await _userRepository.AddUserActivityAsync(activity, cancellationToken);

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

        private string? GetIpAddress()
        {
            // This should be injected from HttpContext
            return "127.0.0.1";
        }
    }
}
