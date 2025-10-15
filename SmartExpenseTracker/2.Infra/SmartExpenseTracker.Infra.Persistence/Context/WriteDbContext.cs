
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartExpenseTracker.Core.Domain.DomainModels.Budgets;
using SmartExpenseTracker.Core.Domain.DomainModels.Common;
using SmartExpenseTracker.Core.Domain.DomainModels.Identity;

namespace SmartExpenseTracker.Infra.Persistence.Context
{
    public class WriteDbContext : IdentityDbContext<
    ApplicationUser,
    ApplicationRole,
    Guid,
    IdentityUserClaim<Guid>,
    ApplicationUserRole,
    IdentityUserLogin<Guid>,
    IdentityRoleClaim<Guid>,
    IdentityUserToken<Guid>>
    {
        public WriteDbContext(DbContextOptions<WriteDbContext> options) : base(options)
        {
        }

        public DbSet<Budget> Budgets => Set<Budget>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            
            builder.ApplyConfigurationsFromAssembly(typeof(WriteDbContext).Assembly);
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("Users");
                entity.Property(u => u.FirstName).HasMaxLength(50).IsRequired();
                entity.Property(u => u.LastName).HasMaxLength(50).IsRequired();
            });

            builder.Entity<ApplicationRole>(entity =>
            {
                entity.ToTable("Roles");
            });

            builder.Entity<ApplicationUserRole>(entity =>
            {
                entity.ToTable("UserRoles");

                entity.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();

                entity.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();
            });

            builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

            SeedRoles(builder);
        }

    

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Publish domain events before saving
            var domainEvents = ChangeTracker.Entries<BaseEntity>()
                .SelectMany(x => x.Entity.DomainEvents)
                .ToList();

            var result = await base.SaveChangesAsync(cancellationToken);

            // Dispatch domain events after successful save
            // This should be handled by MediatR in your application layer

            return result;
        }

        private void SeedRoles(ModelBuilder builder)
        {
            var roles = new List<ApplicationRole>
        {
            new ApplicationRole("مدیر سیستم")
            {
                Id = Guid.Parse("a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d"),
                Name = "Admin",
                NormalizedName = "ADMIN",
                ConcurrencyStamp = Guid.NewGuid().ToString()
            },
            new ApplicationRole("کاربر عادی")
            {
                Id = Guid.Parse("b2c3d4e5-f6a7-4b6c-9d0e-1f2a3b4c5d6e"),
                Name = "User",
                NormalizedName = "USER",
                ConcurrencyStamp = Guid.NewGuid().ToString()
            }
        };

            builder.Entity<ApplicationRole>().HasData(roles);
        }
    }
}
