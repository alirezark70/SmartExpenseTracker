using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.ApplicationService.Dtos.Identity
{
    public record RefreshTokenRequestDto
    {
        [Required(ErrorMessage = "توکن دسترسی الزامی است")]
        public string AccessToken { get; init; } = string.Empty;

        [Required(ErrorMessage = "توکن تازه‌سازی الزامی است")]
        public string RefreshToken { get; init; } = string.Empty;
    }
}
