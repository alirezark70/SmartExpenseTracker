using SmartExpenseTracker.Core.Domain.DomainModels.Common;
using SmartExpenseTracker.Core.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.Domain.DomainModels.Users.Entities
{
    public sealed class User : BaseEntity
    {
        public User(Guid id, DateTime createdAt) : base(id, createdAt)
        {
        }
        private string _email;
        private string _firstName;
        private string _lastName;
        private string _passwordHash;
        private Currency _defaultCurrency;
        private bool _isEmailVerified;
        private DateTime? _lastLoginAt;
        private UserPreferences _preferences;

        public string Email => _email;
        public string FirstName => _firstName;
        public string LastName => _lastName;
        public string FullName => $"{_firstName} {_lastName}";
        public string PasswordHash => _passwordHash;
        public Currency DefaultCurrency => _defaultCurrency;
        public bool IsEmailVerified => _isEmailVerified;
        public DateTime? LastLoginAt => _lastLoginAt;
        public UserPreferences Preferences => _preferences;


        public User(Guid id,
            DateTime createdAt,
            string email,
            string firstName,
            string lastName,
            string passwordHash,
            Currency defaultCurrency) :base(id, createdAt)
        {
            SetEmail(email);
            SetFirstName(firstName);
            SetLastName(lastName);
            _passwordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
            _defaultCurrency = defaultCurrency ?? Currency.USD;
            _isEmailVerified = false;
            _preferences = new UserPreferences();
        }

        public void UpdateProfile(string firstName, string lastName)
        {
            SetFirstName(firstName);
            SetLastName(lastName);
            SetUpdateTime(CreatedAt);
        }

        public void ChangePassword(string newPasswordHash)
        {
            if (string.IsNullOrWhiteSpace(newPasswordHash))
                throw new ArgumentException("Password hash cannot be empty");

            _passwordHash = newPasswordHash;
            SetUpdateTime(CreatedAt);
        }

        public void VerifyEmail()
        {
            _isEmailVerified = true;
            SetUpdateTime(CreatedAt);
        }

        public void RecordLogin()
        {
            _lastLoginAt = DateTime.UtcNow;
            SetUpdateTime(CreatedAt);
        }

        public void UpdateDefaultCurrency(Currency currency)
        {
            _defaultCurrency = currency ?? throw new ArgumentNullException(nameof(currency));
            SetUpdateTime(CreatedAt);
        }

        public void UpdatePreferences(UserPreferences preferences)
        {
            _preferences = preferences ?? throw new ArgumentNullException(nameof(preferences));
            SetUpdateTime(CreatedAt);
        }

        private void SetEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty");

            if (!EmailAddress.IsValid(email))
                throw new ArgumentException("Invalid email format");

            _email = email.ToLowerInvariant();
        }

        private void SetFirstName(string firstName)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("First name cannot be empty");

            if (firstName.Length > 50)
                throw new ArgumentException("First name cannot exceed 50 characters");

            _firstName = firstName;
        }

        private void SetLastName(string lastName)
        {
            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Last name cannot be empty");

            if (lastName.Length > 50)
                throw new ArgumentException("Last name cannot exceed 50 characters");

            _lastName = lastName;
        }
    }
}
