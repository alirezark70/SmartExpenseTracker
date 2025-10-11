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
    public class UserLoginHistoryConfiguration : IEntityTypeConfiguration<UserLoginHistory>
    {
        public void Configure(EntityTypeBuilder<UserLoginHistory> builder)
        {
            builder.ToTable("UserLoginHistories", "Identity");

            builder.HasKey(h => h.Id);

            builder.Property(h => h.IpAddress)
                .IsRequired()
                .HasMaxLength(45);

            builder.Property(h => h.UserAgent)
                .HasMaxLength(500);

            builder.Property(h => h.FailureReason)
                .HasMaxLength(200);

            // Indexes
            builder.HasIndex(h => h.UserId)
                .HasDatabaseName("IX_UserLoginHistories_UserId");

            builder.HasIndex(h => h.LoginTime)
                .HasDatabaseName("IX_UserLoginHistories_LoginTime");

            builder.HasIndex(h => new { h.UserId, h.LoginTime })
                .HasDatabaseName("IX_UserLoginHistories_UserLogin");

            // Navigation
            builder.HasOne(h => h.User)
                .WithMany(u => u.LoginHistories)
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
