using SmartExpenseTracker.Core.Domain.ValueObjects.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.Domain.ValueObjects
{
    public class DateRange : ValueObject
    {
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }
        public int TotalDays => (EndDate - StartDate).Days + 1;

        public DateRange(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
                throw new ArgumentException("Start date cannot be after end date");

            StartDate = startDate.Date;
            EndDate = endDate.Date;
        }

        public bool Contains(DateTime date)
        {
            return date.Date >= StartDate && date.Date <= EndDate;
        }

        public bool Overlaps(DateRange other)
        {
            return StartDate <= other.EndDate && EndDate >= other.StartDate;
        }

        public static DateRange CurrentMonth()
        {
            var now = DateTime.UtcNow;
            var startDate = new DateTime(now.Year, now.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            return new DateRange(startDate, endDate);
        }

        public static DateRange CurrentYear()
        {
            var now = DateTime.UtcNow;
            var startDate = new DateTime(now.Year, 1, 1);
            var endDate = new DateTime(now.Year, 12, 31);
            return new DateRange(startDate, endDate);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return StartDate;
            yield return EndDate;
        }

        public override string ToString() => $"{StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}";

    }
}
