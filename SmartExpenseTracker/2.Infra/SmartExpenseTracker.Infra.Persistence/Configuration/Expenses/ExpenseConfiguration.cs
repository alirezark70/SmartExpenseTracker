using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartExpenseTracker.Core.Domain.DomainModels.Expenses;
using SmartExpenseTracker.Core.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Infra.Persistence.Configuration.Expenses
{
    public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
    {
        public void Configure(EntityTypeBuilder<Expense> builder)
        {
            builder.ToTable("Expenses", "Financial");

            builder.HasKey(e => e.Id);

            builder.HasIndex(e => e.UserId);
            builder.HasIndex(e => e.ExpenseDate);
            builder.HasIndex(e => new { e.UserId, e.Category });
            builder.HasIndex(e => e.IsDeleted);

            // Value Object: Money
            builder.OwnsOne(e => e.Amount, money =>
            {
                money.Property(m => m.Value)
                    .HasColumnName("Amount")
                    .HasPrecision(18, 2)
                    .IsRequired();

                money.Property(m => m.Currency)
                    .HasColumnName("Currency")
                    .HasMaxLength(3)
                    .IsRequired();
            });

            // Value Object: Location (Optional)
            builder.OwnsOne(e => e.Location, location =>
            {
                location.Property(l => l.Name)
                    .HasColumnName("LocationName")
                    .HasMaxLength(200);

                location.Property(l => l.Latitude)
                    .HasColumnName("Latitude");

                location.Property(l => l.Longitude)
                    .HasColumnName("Longitude");

                location.Property(l => l.Address)
                    .HasColumnName("Address")
                    .HasMaxLength(500);

                location.Property(l => l.City)
                    .HasColumnName("City")
                    .HasMaxLength(100);

                location.Property(l => l.Country)
                    .HasColumnName("Country")
                    .HasMaxLength(100);
            });

            // Value Object: RecurrencePattern (Optional)
            builder.OwnsOne(e => e.RecurrencePattern, pattern =>
            {
                pattern.Property(p => p.Type)
                    .HasColumnName("RecurrenceType");

                pattern.Property(p => p.Interval)
                    .HasColumnName("RecurrenceInterval");

                pattern.Property(p => p.EndDate)
                    .HasColumnName("RecurrenceEndDate");

                pattern.Property(p => p.Occurrences)
                    .HasColumnName("RecurrenceOccurrences");
            });

            // Tags as JSON
            builder.Property(e => e.Tags)
                .HasConversion(
                    tags => System.Text.Json.JsonSerializer.Serialize(tags, (System.Text.Json.JsonSerializerOptions?)null),
                    json => System.Text.Json.JsonSerializer.Deserialize<List<Tag>>(json, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<Tag>()
                )
                .HasColumnName("Tags")
                .HasColumnType("nvarchar(max)");

            // Properties
            builder.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(e => e.Category)
                .IsRequired();

            builder.Property(e => e.PaymentMethod)
                .IsRequired();

            builder.Property(e => e.ReceiptUrl)
                .HasMaxLength(500);

            builder.Property(e => e.ExpenseDate)
                .IsRequired();

            builder.Property(e => e.IsRecurring)
                .IsRequired()
                .HasDefaultValue(false);

            // Ignore DomainEvents
            builder.Ignore(e => e.DomainEvents);
        }
    }
}
