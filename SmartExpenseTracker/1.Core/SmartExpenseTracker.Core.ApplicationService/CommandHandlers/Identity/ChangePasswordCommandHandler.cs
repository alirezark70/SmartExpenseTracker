using Microsoft.AspNetCore.Identity;
using SmartExpenseTracker.Core.ApplicationService.Commands.Identity;
using SmartExpenseTracker.Core.ApplicationService.Contracts.Mediator;
using SmartExpenseTracker.Core.Domain.DomainModels.Identity;
using SmartExpenseTracker.Core.Domain.DomainModels.Response.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.ApplicationService.CommandHandlers.Identity
{
    public class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand, ApiResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ChangePasswordCommandHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ApiResponse> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());

            if (user == null)
                return ApiResponse.Failure("کاربر یافت نشد",Domain.Enums.Response.ResponseStatus.NotFound);

            var changeResult = await _userManager.ChangePasswordAsync(
                user,
                request.Request.CurrentPassword,
                request.Request.NewPassword);

            if (!changeResult.Succeeded)
            {
                var errors = string.Join(", ", changeResult.Errors.Select(e => e.Description));
                return ApiResponse.Failure($"خطا در تغییر رمز عبور: {errors}",Domain.Enums.Response.ResponseStatus.Unauthorized);
            }

            return ApiResponse.Success();
        }
    }
}
