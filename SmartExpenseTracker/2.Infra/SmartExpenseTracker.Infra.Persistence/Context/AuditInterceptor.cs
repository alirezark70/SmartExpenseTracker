using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SmartExpenseTracker.Core.ApplicationService.Contracts;
using SmartExpenseTracker.Core.ApplicationService.Contracts.Base;
using SmartExpenseTracker.Core.Domain.Contracts.Common;
using SmartExpenseTracker.Core.Domain.DomainModels.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Infra.Persistence.Context
{
    public class AuditInterceptor : SaveChangesInterceptor
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ICustomContextAccessor _customContextAccessor;

        public AuditInterceptor(IDateTimeProvider dateTimeProvider, ICustomContextAccessor contextAccessor)
        {
            _dateTimeProvider = dateTimeProvider;
            _customContextAccessor = contextAccessor;
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var dbContext = eventData.Context;
            var currentTime = _dateTimeProvider?.GetDateTimeUtcNow() ?? DateTime.UtcNow;
            var currentUser = _customContextAccessor?.UserId;

            foreach (var entry in dbContext.ChangeTracker.Entries<IBaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        SetAuditFields(entry, currentTime, currentUser, isCreation: true);
                        break;

                    case EntityState.Modified:
                        SetAuditFields(entry, currentTime, currentUser, isCreation: false);
                        break;

                    case EntityState.Deleted:
                        entry.State = EntityState.Modified;
                        SetDeleteFields(entry, currentTime, currentUser);
                        break;
                }
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private static void SetAuditFields(EntityEntry entry, DateTime time, Guid? userId, bool isCreation)
        {
            if (isCreation)
            {
                entry.Property(nameof(BaseEntity.CreatedAt)).CurrentValue = time;
                entry.Property(nameof(BaseEntity.CreatedBy)).CurrentValue = userId;
            }

            entry.Property(nameof(BaseEntity.ModifiedAt)).CurrentValue = time;
            entry.Property(nameof(BaseEntity.ModifiedBy)).CurrentValue = userId;
        }

        private static void SetDeleteFields(EntityEntry entry, DateTime time, Guid? userId)
        {
            entry.Property(nameof(BaseEntity.IsDeleted)).CurrentValue = true;
            entry.Property(nameof(BaseEntity.ModifiedAt)).CurrentValue = time;
            entry.Property(nameof(BaseEntity.ModifiedBy)).CurrentValue = userId;
        }
    }
}
