
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartExpenseTracker.Core.ApplicationService.Contracts;
using SmartExpenseTracker.Core.ApplicationService.Contracts.Base;
using SmartExpenseTracker.Core.Domain.Contracts.Common;
using SmartExpenseTracker.Core.Domain.DomainModels.Budgets;
using SmartExpenseTracker.Core.Domain.DomainModels.Common;
using SmartExpenseTracker.Core.Domain.DomainModels.Expenses;
using SmartExpenseTracker.Core.Domain.DomainModels.Identity;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

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
        private readonly ICustomContextAccessor? _contextAccessor;
        private readonly IDateTimeProvider? _dateTimeProvider;
        private readonly AuditInterceptor _auditInterceptor;
        public WriteDbContext(
            DbContextOptions<WriteDbContext> options,
            ICustomContextAccessor? contextAccessor, IDateTimeProvider? dateTimeProvider, AuditInterceptor auditInterceptor) : base(options)
        {
            _contextAccessor = contextAccessor;
            _dateTimeProvider = dateTimeProvider;
            _auditInterceptor = auditInterceptor;
            _auditInterceptor = auditInterceptor;
        }

        public WriteDbContext(DbContextOptions<WriteDbContext> options) : base(options)
        {
        }

        public DbSet<Expense> Expenses => Set<Expense>();
        public DbSet<Budget> Budgets => Set<Budget>();
        public DbSet<BudgetAlert> BudgetAlerts => Set<BudgetAlert>();
        public DbSet<UserLoginHistory> UserLoginHistories => Set<UserLoginHistory>();
        public DbSet<UserActivity> UserActivities => Set<UserActivity>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.AddInterceptors(_auditInterceptor);
        }

       
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            builder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("Users", "Identity");
                entity.Property(u => u.FirstName).HasMaxLength(50).IsRequired();
                entity.Property(u => u.LastName).HasMaxLength(50).IsRequired();
            });

            //foreach(var entityType in builder.Model.GetEntityTypes())
            //{
            //    if(typeof(IBaseEntity).IsAssignableFrom(entityType.ClrType))
            //    {
            //        var b = builder.Entity(entityType.ClrType);

            //        b.Property(nameof(BaseEntity.CreatedAt)).HasField("_createdAt");
            //        b.Property(nameof(BaseEntity.CreatedBy)).HasField("_createdBy");
            //        b.Property(nameof(BaseEntity.ModifiedAt)).HasField("_modifiedAt");
            //        b.Property(nameof(BaseEntity.ModifiedBy)).HasField("_modifiedBy");
            //        b.Property(nameof(BaseEntity.IsDeleted)).HasField("_isDeleted");
            //    }
            //}

            foreach (var entityType in builder.Model.GetEntityTypes()
                 .Where(t => typeof(IBaseEntity).IsAssignableFrom(t.ClrType)))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var prop = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var body = Expression.Equal(prop, Expression.Constant(false));
                var lambda = Expression.Lambda(body, parameter);
                builder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }



            builder.Entity<ApplicationRole>(entity =>
            {
                entity.ToTable("Roles", "Identity");
            });

            builder.Entity<ApplicationUserRole>(entity =>
            {
                entity.ToTable("UserRoles", "Identity");

                entity.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();

                entity.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();
            });

            builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims", "Identity");
            builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins", "Identity");
            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims", "Identity");
            builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens","Identity");

            SeedRoles(builder);
        }

    

       


        //private async Task BeforeSaveChangesAsync()
        //{
        //    var entries = ChangeTracker.Entries<IBaseEntity>()
        //        .Where(e => e.State is EntityState.Added
        //                 or EntityState.Modified
        //                 or EntityState.Deleted);

        //    var currentTime = _dateTimeProvider?.GetDateTimeUtcNow() ?? DateTime.UtcNow;
        //    var currentUser = _contextAccessor?.UserId;

        //    foreach (var entry in entries)
        //    {
        //        switch (entry.State)
        //        {
        //            case EntityState.Added:
        //                entry.Property(nameof(BaseEntity.CreatedAt)).CurrentValue = currentTime;
        //                entry.Property(nameof(BaseEntity.CreatedBy)).CurrentValue = currentUser;                    
        //                break;

        //            case EntityState.Modified:
        //                entry.Property(nameof(BaseEntity.ModifiedAt)).CurrentValue = currentTime;
        //                entry.Property(nameof(BaseEntity.ModifiedBy)).CurrentValue = currentUser;
        //                break;

        //            case EntityState.Deleted:
        //                entry.State = EntityState.Modified;
        //                entry.Property(nameof(BaseEntity.IsDeleted)).CurrentValue = true;
        //                entry.Property(nameof(BaseEntity.ModifiedAt)).CurrentValue = currentTime;
        //                entry.Property(nameof(BaseEntity.ModifiedBy)).CurrentValue = currentUser;
        //                break;
        //        }
        //    }

        //    await Task.CompletedTask;
        //}

        public async Task PublishDomainEventsAsync(CancellationToken cancellationToken = default)
        {
            var entities = ChangeTracker.Entries<IBaseEntity>()
                .Select(e => e.Entity)
                .Where(e => e.DomainEvents.Any())
                .ToList();

            var events = entities.SelectMany(e => e.DomainEvents).ToList();

            foreach (var entity in entities)
            {
                entity.ClearDomainEvents();
            }

            // Publish events (implement your event publisher here)
            foreach (var @event in events)
            {
                // await _mediator.Publish(@event, cancellationToken);
            }

            await Task.CompletedTask;
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
                ConcurrencyStamp = "a1b2c3d4-e5f6-4a5b-1c9d-0e1f253b4c5d"
            },
            new ApplicationRole("کاربر عادی")
            {
                Id = Guid.Parse("b2c3d4e5-f6a7-4b6c-9d0e-1f2a3b4c5d6e"),
                Name = "User",
                NormalizedName = "USER",
                ConcurrencyStamp = "b2c374e5-f6a7-4b6c-950e-1f2a3b4c5d6e"
            }
        };

            builder.Entity<ApplicationRole>().HasData(roles);
        }

    }
}
