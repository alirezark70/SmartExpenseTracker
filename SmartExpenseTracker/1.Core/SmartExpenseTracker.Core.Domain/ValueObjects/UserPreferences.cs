using SmartExpenseTracker.Core.Domain.ValueObjects.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.Domain.ValueObjects
{
    public class UserPreferences : ValueObject
    {
        public bool EnableNotifications { get; }
        public bool EnableBudgetAlerts { get; }
        public string DateFormat { get; }
        public string TimeZone { get; }
        public string Language { get; }

        public UserPreferences(
            bool enableNotifications = true,
            bool enableBudgetAlerts = true,
            string dateFormat = "yyyy-MM-dd",
            string timeZone = "Asia/Tehran",
            string language = "fa-IR")
        {
            EnableNotifications = enableNotifications;
            EnableBudgetAlerts = enableBudgetAlerts;
            DateFormat = dateFormat ?? "yyyy-MM-dd";
            TimeZone = timeZone ?? "Asia/Tehran";
            Language = language ?? "fa-IR";
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return EnableNotifications;
            yield return EnableBudgetAlerts;
            yield return DateFormat;
            yield return TimeZone;
            yield return Language;
        }
    }

}
