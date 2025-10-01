using SmartExpenseTracker.Core.Domain.ValueObjects.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.Domain.ValueObjects
{
    public class Money : ValueObject
    {
        public static Money Zero(string currency) => new Money(0, currency);
        public override string ToString() => $"{Value:N2} {Currency}";


        public decimal Value { get; }
        public string Currency { get; }

        public Money(decimal value, string currency)
        {
            if (value < 0)
                throw new ArgumentException("Money value cannot be negative");

            if (string.IsNullOrWhiteSpace(currency))
                throw new ArgumentException("Currency cannot be empty");

            if (currency.Length != 3)
                throw new ArgumentException("Currency must be a 3-letter ISO code");

            Value = Math.Round(value, 2);
            Currency = currency.ToUpper();
        }

        public Money Add(Money other)
        {
            if (Currency != other.Currency)
                throw new InvalidOperationException($"Cannot add money with different currencies: {Currency} and {other.Currency}");

            return new Money(Value + other.Value, Currency);
        }

        public Money Subtract(Money other)
        {
            if (Currency != other.Currency)
                throw new InvalidOperationException($"Cannot subtract money with different currencies: {Currency} and {other.Currency}");

            return new Money(Value - other.Value, Currency);
        }

        public Money Multiply(decimal factor)
        {
            return new Money(Value * factor, Currency);
        }

        public bool IsGreaterThan(Money other)
        {
            if (Currency != other.Currency)
                throw new InvalidOperationException($"Cannot compare money with different currencies: {Currency} and {other.Currency}");

            return Value > other.Value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
            yield return Currency;
        }


    }
}
