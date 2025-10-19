using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartExpenseTracker.Core.Domain.DomainModels.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Infra.Persistence.EntityConfigurations
{
    public class UserActivityConfiguration : IEntityTypeConfiguration<UserActivity>
    {
        public void Configure(EntityTypeBuilder<UserActivity> builder)
        {
            builder.ToTable("UserActivities", "Identity");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.ActivityType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(a => a.Description)
                .HasMaxLength(500);

            builder.Property(a => a.IpAddress)
                .HasMaxLength(45);

            builder.Property(a => a.OccurredAt)
                .IsRequired();

            builder.Property(a => a.MetaData)
                .HasColumnType("nvarchar(max)");

            // Indexes
            builder.HasIndex(a => a.UserId)
                .HasDatabaseName("IX_UserActivities_UserId");

            builder.HasIndex(a => a.OccurredAt)
                .HasDatabaseName("IX_UserActivities_OccurredAt");

            builder.HasIndex(a => a.ActivityType)
                .HasDatabaseName("IX_UserActivities_ActivityType");

            builder.HasIndex(a => new { a.UserId, a.OccurredAt })
                .HasDatabaseName("IX_UserActivities_UserOccurred");

            // Navigation
            builder.HasOne(a => a.User)
                .WithMany(u => u.Activities)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Query Filters
            builder.HasQueryFilter(a => !a.IsDeleted);
        }
    }
}
