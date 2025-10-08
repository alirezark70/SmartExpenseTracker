using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.ApplicationService.Dtos.Identity
{
    public record RegisterRequestDto
    {
        [Required(ErrorMessage = "نام کاربری الزامی است")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "نام کاربری باید بین 3 تا 50 کاراکتر باشد")]
        public string UserName { get; init; } = string.Empty;

        [Required(ErrorMessage = "ایمیل الزامی است")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل صحیح نیست")]
        public string Email { get; init; } = string.Empty;

        [Required(ErrorMessage = "رمز عبور الزامی است")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "رمز عبور باید حداقل 6 کاراکتر باشد")]
        public string Password { get; init; } = string.Empty;

        [Required(ErrorMessage = "تایید رمز عبور الزامی است")]
        [Compare(nameof(Password), ErrorMessage = "رمز عبور و تایید آن مطابقت ندارند")]
        public string ConfirmPassword { get; init; } = string.Empty;

        [Required(ErrorMessage = "نام الزامی است")]
        [StringLength(50, ErrorMessage = "نام نباید بیشتر از 50 کاراکتر باشد")]
        public string FirstName { get; init; } = string.Empty;

        [Required(ErrorMessage = "نام خانوادگی الزامی است")]
        [StringLength(50, ErrorMessage = "نام خانوادگی نباید بیشتر از 50 کاراکتر باشد")]
        public string LastName { get; init; } = string.Empty;

        [Phone(ErrorMessage = "شماره تلفن معتبر نیست")]
        public string? PhoneNumber { get; init; }
    }
}
