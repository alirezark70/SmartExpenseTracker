using SmartExpenseTracker.Core.Domain.DomainModels.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.Domain.DomainModels.Identity
{
    public class UserLoginHistory : BaseEntity
    {
        public Guid UserId { get; private set; }
        public string IpAddress { get; private set; }
        public string? UserAgent { get; private set; }
        public DateTime LoginTime { get; private set; }
        public bool IsSuccessful { get; private set; }
        public string? FailureReason { get; private set; }

        public virtual ApplicationUser User { get; private set; } = null!;

        public UserLoginHistory(
            Guid id,
            Guid userId,
            string ipAddress,
            string? userAgent,
            bool isSuccessful,
            string? failureReason = null) :base(id)
        {
            Id = id;
            UserId = userId;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            IsSuccessful = isSuccessful;
            FailureReason = failureReason;
        }

        private UserLoginHistory() { } // For EF Core
    }
}
