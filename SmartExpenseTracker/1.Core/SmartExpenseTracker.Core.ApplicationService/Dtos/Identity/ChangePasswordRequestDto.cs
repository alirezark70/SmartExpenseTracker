using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.ApplicationService.Dtos.Identity
{
    public record ChangePasswordRequestDto
    {
        [Required(ErrorMessage = "رمز عبور فعلی الزامی است")]
        public string CurrentPassword { get; init; } = string.Empty;

        [Required(ErrorMessage = "رمز عبور جدید الزامی است")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "رمز عبور جدید باید حداقل 6 کاراکتر باشد")]
        public string NewPassword { get; init; } = string.Empty;

        [Required(ErrorMessage = "تایید رمز عبور جدید الزامی است")]
        [Compare(nameof(NewPassword), ErrorMessage = "رمز عبور جدید و تایید آن مطابقت ندارند")]
        public string ConfirmNewPassword { get; init; } = string.Empty;
    }
}
