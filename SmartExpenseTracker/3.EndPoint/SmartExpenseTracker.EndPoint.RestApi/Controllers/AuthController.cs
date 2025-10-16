using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartExpenseTracker.Core.ApplicationService.Commands.Identity;
using SmartExpenseTracker.Core.ApplicationService.Contracts;
using SmartExpenseTracker.Core.ApplicationService.DataTransfareObject.Users;
using SmartExpenseTracker.Core.ApplicationService.Dtos.Identity;
using SmartExpenseTracker.Core.Domain.Contracts.Common;
using SmartExpenseTracker.Core.Domain.DomainModels.Response.Entities;
using SmartExpenseTracker.EndPoint.RestApi.Controllers.Base;
using SmartExpenseTracker.Infra.Mapping.Contracts;
using System.Security.Claims;

namespace SmartExpenseTracker.EndPoint.RestApi.Controllers
{

    public class AuthController : BaseApiController
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IDateTimeProvider _dateTimeProvider;
        public AuthController(ILogger<PostController> logger, IMappingService mappingService, IMediator mediator, IDateTimeProvider dateTimeProvider, ICurrentUserService currentUserService) : base(logger, mappingService, mediator)
        {
            _dateTimeProvider = dateTimeProvider;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// ثبت نام کاربر جدید
        /// </summary>
        [HttpPost("[Action]")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                var command = new RegisterCommand(request);
                var result = await _mediator.Send(command, cancellationToken);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("New user registered: {UserName}", request.UserName);
                    return CreatedAtAction(nameof(GetCurrentUser), new { }, result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for user: {UserName}", request.UserName);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ErrorResponse.Create("خطایی در ثبت نام رخ داده است"));
            }
        }

        /// <summary>
        /// دریافت اطلاعات کاربر فعلی
        /// </summary>
        [HttpGet("[Action]")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<CurrentUserDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
        {
            try
            {
                var currentUser = _currentUserService.GetCurrentUser(default);

                var currentUserDto = _mappingService.Map<CurrentUserDto>(currentUser);

                return Ok(ApiResponse<CurrentUserDto>.Success(currentUserDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user info");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ErrorResponse.Create("خطایی در دریافت اطلاعات کاربر رخ داده است"));
            }
        }

        /// <summary>
        /// ورود کاربر
        /// </summary>
        [HttpPost("[Action]")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
                var command = new LoginCommand(request);
                var result = await _mediator.Send(command, cancellationToken);

                if (result.IsSuccess)
                {
                    SetRefreshTokenCookie(result.Data!.RefreshToken);
                    _logger.LogInformation("User logged in: {UserName}", request.UserNameOrEmail);
                    return Ok(result);
                }

                return Unauthorized(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user: {UserName}", request.UserNameOrEmail);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ErrorResponse.Create("خطایی در ورود رخ داده است"));
            }
        }

        /// <summary>
        /// تازه‌سازی توکن
        /// </summary>
        [HttpPost("[Action]")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                // اگر RefreshToken در Cookie است
                var refreshToken = request.RefreshToken;
                if (string.IsNullOrEmpty(refreshToken))
                {
                    refreshToken = Request.Cookies["refreshToken"];
                }

                if (string.IsNullOrEmpty(refreshToken))
                {
                    return Unauthorized(ErrorResponse.Create("توکن تازه‌سازی یافت نشد"));
                }

                var command = new RefreshTokenCommand(new RefreshTokenRequestDto
                {
                    AccessToken = request.AccessToken,
                    RefreshToken = refreshToken
                });

                var result = await _mediator.Send(command, cancellationToken);

                if (result.IsSuccess)
                {
                    SetRefreshTokenCookie(result.Data!.RefreshToken);
                    return Ok(result);
                }

                return Unauthorized(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ErrorResponse.Create("خطایی در تازه‌سازی توکن رخ داده است"));
            }
        }
        /// <summary>
        /// خروج کاربر
        /// </summary>
        [HttpPost("[Action]")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userId, out var userGuid))
                {
                    return Unauthorized(ErrorResponse.Create("کاربر یافت نشد"));
                }

                var command = new LogoutCommand(userGuid);
                var result = await _mediator.Send(command, cancellationToken);

                if (result.IsSuccess)
                {
                    RemoveRefreshTokenCookie();
                    _logger.LogInformation("User logged out: {UserId}", userId);
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ErrorResponse.Create("خطایی در خروج رخ داده است"));
            }
        }

        /// <summary>
        /// تغییر رمز عبور
        /// </summary>
        [HttpPost("[Action]")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userId, out var userGuid))
                {
                    return Unauthorized(ErrorResponse.Create("کاربر یافت نشد"));
                }

                var command = new ChangePasswordCommand(userGuid, request);
                var result = await _mediator.Send(command, cancellationToken);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Password changed for user: {UserId}", userId);
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password change");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ErrorResponse.Create("خطایی در تغییر رمز عبور رخ داده است"));
            }
        }


        #region Private Methods

        private void SetRefreshTokenCookie(string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // در Production باید true باشد
                SameSite = SameSiteMode.Strict,
                Expires = _dateTimeProvider.GetDateTimeNow().AddDays(7)
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }

        private void RemoveRefreshTokenCookie()
        {
            Response.Cookies.Delete("refreshToken");
        }

        #endregion
    }
}
