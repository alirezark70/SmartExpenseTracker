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
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.ToTable("Users", "Identity");

            // Primary Key
            builder.HasKey(u => u.Id);

            // Properties
            builder.Property(u => u.FirstName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.LastName)
                .IsRequired()
                .HasMaxLength(50);

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

            // Indexes for Performance
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

            builder.HasIndex(u => u.IsActive)
                .HasDatabaseName("IX_Users_IsActive");

            builder.HasIndex(u => new { u.RefreshTokenHash, u.RefreshTokenExpiryTime })
                .HasDatabaseName("IX_Users_RefreshToken")
                .HasFilter("[RefreshTokenHash] IS NOT NULL");


            // Composite Index for Login
            builder.HasIndex(u => new { u.UserName, u.IsActive })
                .HasDatabaseName("IX_Users_Login");

            // Navigation Properties
            builder.HasMany(u => u.UserRoles)
                .WithOne(ur => ur.User)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.LoginHistories)
                .WithOne(h => h.User)
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.Activities)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ignore computed properties
            builder.Ignore(u => u.DomainEvents);

            // Query Filters
            builder.HasQueryFilter(u => !u.IsDeleted);
        }
    }
}
