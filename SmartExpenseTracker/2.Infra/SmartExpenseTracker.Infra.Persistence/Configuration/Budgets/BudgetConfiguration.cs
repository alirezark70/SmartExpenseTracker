using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartExpenseTracker.Core.Domain.DomainModels.Budgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Infra.Persistence.Configuration.Budgets
{
    public class BudgetConfiguration : IEntityTypeConfiguration<Budget>
    {
        public void Configure(EntityTypeBuilder<Budget> builder)
        {
            builder.ToTable("Budgets", "Financial");

            builder.HasKey(b => b.Id);

            builder.HasIndex(b => b.UserId);
            builder.HasIndex(b => new { b.UserId, b.Category });
            builder.HasIndex(b => b.IsDeleted);

            // Value Object: Money for Limit
            builder.OwnsOne(b => b.Limit, money =>
            {
                money.Property(m => m.Value)
                    .HasColumnName("LimitAmount")
                    .HasPrecision(18, 2)
                    .IsRequired();

                money.Property(m => m.Currency)
                    .HasColumnName("LimitCurrency")
                    .HasMaxLength(3)
                    .IsRequired();
            });

            // Value Object: Money for Spent
            builder.OwnsOne(b => b.Spent, money =>
            {
                money.Property(m => m.Value)
                    .HasColumnName("SpentAmount")
                    .HasPrecision(18, 2)
                    .IsRequired();

                money.Property(m => m.Currency)
                    .HasColumnName("SpentCurrency")
                    .HasMaxLength(3)
                    .IsRequired();
            });

            // Value Object: DateRange
            builder.OwnsOne(b => b.Period, period =>
            {
                period.Property(p => p.StartDate)
                    .HasColumnName("PeriodStartDate")
                    .IsRequired();

                period.Property(p => p.EndDate)
                    .HasColumnName("PeriodEndDate")
                    .IsRequired();
            });

            // Properties
            builder.Property(b => b.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(b => b.Category)
                .IsRequired();

            builder.Property(b => b.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Relationships
            builder.HasMany(b => b.Alerts)
                .WithOne()
                .HasForeignKey(a => a.BudgetId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ignore computed properties and domain events
            builder.Ignore(b => b.Remaining);
            builder.Ignore(b => b.UsagePercentage);
            builder.Ignore(b => b.DomainEvents);
        }
    }
}
