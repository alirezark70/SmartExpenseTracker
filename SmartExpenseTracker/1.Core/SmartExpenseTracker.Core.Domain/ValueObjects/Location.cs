using SmartExpenseTracker.Core.Domain.ValueObjects.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.Domain.ValueObjects
{
    public class Location : ValueObject
    {
        public bool HasCoordinates => Latitude.HasValue && Longitude.HasValue;
        public Location(string name, double? latitude = null, double? longitude = null,
        string? address = null, string? city = null, string? country = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Location name cannot be empty");

            if (latitude.HasValue && (latitude < -90 || latitude > 90))
                throw new ArgumentException("Latitude must be between -90 and 90");

            if (longitude.HasValue && (longitude < -180 || longitude > 180))
                throw new ArgumentException("Longitude must be between -180 and 180");

            Name = name;
            Latitude = latitude;
            Longitude = longitude;
            Address = address;
            City = city;
            Country = country;
        }
        public string Name { get; }
        public double? Latitude { get; }
        public double? Longitude { get; }
        public string? Address { get; }
        public string? City { get; }
        public string? Country { get; }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Name;
            yield return Latitude ?? 0;
            yield return Longitude ?? 0;
            yield return Address ?? string.Empty;
            yield return City ?? string.Empty;
            yield return Country ?? string.Empty;
        }

        public override string ToString()
        {
            var parts = new List<string> { Name };

            if (!string.IsNullOrWhiteSpace(City))
                parts.Add(City);

            if (!string.IsNullOrWhiteSpace(Country))
                parts.Add(Country);

            return string.Join(", ", parts);
        }
    }
}
