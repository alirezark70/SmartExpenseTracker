using SmartExpenseTracker.Core.Domain.ValueObjects.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.Domain.ValueObjects
{
    public class Tag : ValueObject
    {
        public string Name { get; }
        public string? Color { get; }


        public Tag(string name, string? color = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Tag name cannot be empty");

            if (name.Length > 30)
                throw new ArgumentException("Tag name cannot exceed 30 characters");

            if (color != null && !IsValidHexColor(color))
                throw new ArgumentException("Invalid hex color format");

            Name = name.Trim().ToLower();
            Color = color?.ToUpper();
        }
        private static bool IsValidHexColor(string color)
        {
            return Regex.IsMatch(color, @"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$");
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Name;
            yield return Color ?? string.Empty;
        }

        public override string ToString() => Name;
       
    }
}
