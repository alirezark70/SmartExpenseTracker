using SmartExpenseTracker.Core.Domain.ValueObjects.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.Domain.ValueObjects
{
    public class EmailAddress : ValueObject
    {
        public override string ToString() => Value;
        public string Value { get; }

        private static readonly Regex EmailRegex = new Regex(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);


        public EmailAddress(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Email address cannot be empty");

            if (!IsValid(value))
                throw new ArgumentException($"Invalid email address: {value}");

            Value = value.ToLowerInvariant();
        }

        public static bool IsValid(string email)
        {
            return !string.IsNullOrWhiteSpace(email) && EmailRegex.IsMatch(email);
        }
        public static implicit operator string(EmailAddress email) => email.Value;
        public static explicit operator EmailAddress(string value) => new EmailAddress(value);

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        
    }
}
