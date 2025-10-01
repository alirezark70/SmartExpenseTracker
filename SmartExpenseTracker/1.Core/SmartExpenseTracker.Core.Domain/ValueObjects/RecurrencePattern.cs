using SmartExpenseTracker.Core.Domain.Enums.General;
using SmartExpenseTracker.Core.Domain.ValueObjects.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.Domain.ValueObjects
{
    public class RecurrencePattern : ValueObject
    {
        public RecurrencePattern(RecurrenceType type, int interval = 1,
        DateTime? endDate = null, int? occurrences = null)
        {
            if (interval <= 0)
                throw new ArgumentException("Interval must be greater than zero");

            if (endDate.HasValue && endDate.Value <= DateTime.UtcNow)
                throw new ArgumentException("End date must be in the future");

            if (occurrences.HasValue && occurrences.Value <= 0)
                throw new ArgumentException("Occurrences must be greater than zero");

            Type = type;
            Interval = interval;
            EndDate = endDate;
            Occurrences = occurrences;
        }

        public DateTime GetNextOccurrence(DateTime fromDate)
        {
            return Type switch
            {
                RecurrenceType.Daily => fromDate.AddDays(Interval),
                RecurrenceType.Weekly => fromDate.AddDays(7 * Interval),
                RecurrenceType.Monthly => fromDate.AddMonths(Interval),
                RecurrenceType.Yearly => fromDate.AddYears(Interval),
                _ => throw new NotSupportedException($"Recurrence type {Type} is not supported")
            };
        }

        public bool ShouldContinue(DateTime currentDate, int currentOccurrenceCount)
        {
            if (EndDate.HasValue && currentDate > EndDate.Value)
                return false;

            if (Occurrences.HasValue && currentOccurrenceCount >= Occurrences.Value)
                return false;

            return true;
        }

        public RecurrenceType Type { get; }
        public int Interval { get; }
        public DateTime? EndDate { get; }
        public int? Occurrences { get; }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Type;
            yield return Interval;
            yield return EndDate ?? DateTime.MinValue;
            yield return Occurrences ?? 0;
        }
    }
}
