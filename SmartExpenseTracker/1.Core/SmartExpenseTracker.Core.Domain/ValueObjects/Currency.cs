using SmartExpenseTracker.Core.Domain.ValueObjects.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.Domain.ValueObjects
{
    public class Currency : ValueObject
    {
        public override string ToString() => Code;

        private Currency(string code, string name, string symbol)
        {
            Code = code;
            Name = name;
            Symbol = symbol;
        }
        public string Code { get; }
        public string Name { get; }
        public string Symbol { get; }

        public static Currency USD => _currencies["USD"];
        public static Currency EUR => _currencies["EUR"];
        public static Currency GBP => _currencies["GBP"];
        public static Currency IRR => _currencies["IRR"];
        public static Currency AED => _currencies["AED"];

        private static readonly Dictionary<string, Currency> _currencies = new()
        {
            ["USD"] = new Currency("USD", "US Dollar", "$"),
            ["EUR"] = new Currency("EUR", "Euro", "€"),
            ["GBP"] = new Currency("GBP", "British Pound", "£"),
            ["IRR"] = new Currency("IRR", "Iranian Rial", "﷼"),
        };

        public static Currency FromCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Currency code cannot be empty");

            code = code.ToUpper();

            if (!_currencies.ContainsKey(code))
                throw new ArgumentException($"Unknown currency code: {code}");

            return _currencies[code];
        }
        public static bool IsValidCode(string code)
        {
            return !string.IsNullOrWhiteSpace(code) && _currencies.ContainsKey(code.ToUpper());
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Code;
        }

    }
}
