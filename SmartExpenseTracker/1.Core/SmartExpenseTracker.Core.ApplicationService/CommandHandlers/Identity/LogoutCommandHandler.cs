using Microsoft.AspNetCore.Identity;
using SmartExpenseTracker.Core.ApplicationService.Commands.Identity;
using SmartExpenseTracker.Core.ApplicationService.Contracts.Mediator;
using SmartExpenseTracker.Core.Domain.DomainModels.Identity;
using SmartExpenseTracker.Core.Domain.DomainModels.Response.Entities;

namespace SmartExpenseTracker.Core.ApplicationService.CommandHandlers.Identity
{
    public class LogoutCommandHandler : ICommandHandler<LogoutCommand, ApiResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public LogoutCommandHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ApiResponse> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());

            if (user == null)
                return  ApiResponse.Failure("کاربر یافت نشد",Domain.Enums.Response.ResponseStatus.NotFound);

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;

            await _userManager.UpdateAsync(user);

            return ApiResponse.Success();
        }
    }
}
