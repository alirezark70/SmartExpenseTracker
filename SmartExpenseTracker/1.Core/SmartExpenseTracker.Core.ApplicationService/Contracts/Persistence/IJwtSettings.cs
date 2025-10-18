using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.ApplicationService.Contracts.Persistence
{
    public interface IJwtSettings
    {
        public string SecretKey { get; }
        public string Issuer { get; } 
        public string Audience { get; } 
        public int AccessTokenExpirationMinutes { get;}
        public int RefreshTokenExpirationDays { get; }
    }
}
