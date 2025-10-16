using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.ApplicationService.Extensions.Helper
{
    public static class EnumExtensions
    {
        public static T? ToEnumOrDefault<T>(this string value) where T : struct, Enum
        {
            return Enum.TryParse<T>(value, true, out var result) ? result : null;
        }
    }
}
