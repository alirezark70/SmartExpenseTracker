using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.ApplicationService.Dtos.Identity
{
    public record LoginRequestDto
    {
        [Required(ErrorMessage = "نام کاربری یا ایمیل الزامی است")]
        public string UserNameOrEmail { get; init; } = string.Empty;

        [Required(ErrorMessage = "رمز عبور الزامی است")]
        public string Password { get; init; } = string.Empty;

        public bool RememberMe { get; init; } = false;
    }
}
