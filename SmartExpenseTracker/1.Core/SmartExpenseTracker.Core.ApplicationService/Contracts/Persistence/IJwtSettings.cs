using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.ApplicationService.Contracts.Persistence
{
    public interface IJwtSettings
    {
        public string SecretKey { get; set; }
        public string Issuer { get; set; } 
        public string Audience { get; set; } 
        public int AccessTokenExpirationMinutes { get; set; }
        public int RefreshTokenExpirationDays { get; set; }
    }
}
