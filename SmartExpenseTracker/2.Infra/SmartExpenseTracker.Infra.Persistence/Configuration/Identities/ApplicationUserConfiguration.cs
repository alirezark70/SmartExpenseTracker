using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartExpenseTracker.Core.Domain.DomainModels.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Infra.Persistence.Configuration.Identities
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.ToTable("Users", "Identity");

            // Primary Key
            builder.HasKey(u => u.Id);

            // Indexes
            builder.HasIndex(u => u.UserName)
                .IsUnique()
                .HasDatabaseName("IX_Users_UserName");

            builder.HasIndex(u => u.NormalizedUserName)
                .IsUnique()
                .HasDatabaseName("IX_Users_NormalizedUserName");

            builder.HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("IX_Users_Email");

            builder.HasIndex(u => u.NormalizedEmail)
                .IsUnique()
                .HasDatabaseName("IX_Users_NormalizedEmail");

            builder.HasIndex(u => u.RefreshTokenHash)
                .HasDatabaseName("IX_Users_RefreshTokenHash");

            builder.HasIndex(u => u.IsDeleted)
                .HasDatabaseName("IX_Users_IsDeleted");

            builder.HasIndex(u => new { u.IsActive, u.IsDeleted })
                .HasDatabaseName("IX_Users_ActiveDeleted");

            // Properties
            builder.Property(u => u.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.UserName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(u => u.ProfileImageUrl)
                .HasMaxLength(500);

            builder.Property(u => u.RefreshTokenHash)
                .HasMaxLength(88); // Base64 encoded 64 bytes

            builder.Property(u => u.RefreshTokenSalt)
                .HasMaxLength(44); // Base64 encoded 32 bytes

            builder.Property(u => u.LastLoginIp)
                .HasMaxLength(45); // IPv6 max length

            builder.Property(u => u.EmailVerificationToken)
                .HasMaxLength(128);

            builder.Property(u => u.PasswordResetToken)
                .HasMaxLength(128);

            // Value Conversions
            builder.Property(u => u.CreatedAt)
                .HasConversion(
                    v => v,
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            builder.Property(u => u.ModifiedAt)
                .HasConversion(
                    v => v,
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

            // Relationships
            builder.HasMany(u => u.UserRoles)
                .WithOne(ur => ur.User)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.LoginHistories)
                .WithOne(lh => lh.User)
                .HasForeignKey(lh => lh.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.Activities)
                .WithOne(ua => ua.User)
                .HasForeignKey(ua => ua.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Query Filters
            builder.HasQueryFilter(u => !u.IsDeleted);
        }
    }
}
