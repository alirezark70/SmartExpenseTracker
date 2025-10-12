using SmartExpenseTracker.Core.Domain.DomainModels.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.Domain.DomainModels.Identity
{
    public class UserActivity : BaseEntity
    {
        public Guid UserId { get; private set; }
        public string ActivityType { get; private set; }
        public string? Description { get; private set; }
        public string? IpAddress { get; private set; }
        public DateTime OccurredAt { get; private set; }
        public string? MetaData { get; private set; } // JSON data

        public virtual ApplicationUser User { get; private set; } = null!;

        public UserActivity(
            Guid id,
            Guid userId,
            string activityType,
            string? description = null,
            string? ipAddress = null,
            string? metaData = null) :base(id)
        {
            Id = id;
            UserId = userId;
            ActivityType = activityType;
            Description = description;
            IpAddress = ipAddress;
            MetaData = metaData;
        }

        private UserActivity() { } // For EF Core
    }
}

