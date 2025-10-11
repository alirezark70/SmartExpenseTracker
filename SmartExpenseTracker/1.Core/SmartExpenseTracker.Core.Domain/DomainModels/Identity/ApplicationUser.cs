using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.Domain.DomainModels.Identity
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        // Basic Information
        public string FirstName { get; private set; } = string.Empty;
        public string LastName { get; private set; } = string.Empty;
        public string? ProfileImageUrl { get; private set; }

        // Status & Activity
        public bool IsActive { get; private set; } = true;
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public DateTime? LastLoginAt { get; private set; }
        public string? LastLoginIp { get; private set; }
        public int FailedLoginAttempts { get; private set; }
        public DateTime? LockoutEndDate { get; private set; }

        // Security Tokens
        public string? RefreshTokenHash { get; private set; }
        public string? RefreshTokenSalt { get; private set; }
        public DateTime? RefreshTokenExpiryTime { get; private set; }

        // Email Verification
        public string? EmailVerificationToken { get; private set; }
        public DateTime? EmailVerificationTokenExpiry { get; private set; }
        public bool EmailVerified { get; private set; }

        // Password Reset
        public string? PasswordResetToken { get; private set; }
        public DateTime? PasswordResetTokenExpiry { get; private set; }

        // Navigation Properties
        public virtual ICollection<ApplicationUserRole> UserRoles { get; private set; }
            = new HashSet<ApplicationUserRole>();
        public virtual ICollection<UserLoginHistory> LoginHistories { get; private set; }
            = new HashSet<UserLoginHistory>();
        public virtual ICollection<UserActivity> Activities { get; private set; }
            = new HashSet<UserActivity>();

        // Constructor
        private ApplicationUser() { } // For EF Core

        public ApplicationUser(
            Guid id,
            DateTime createdAt,
            string userName,
            string email,
            string firstName,
            string lastName,
            string? phoneNumber = null)
        {
            Id = id;
            UserName = userName;
            NormalizedUserName = userName.ToUpperInvariant();
            Email = email;
            NormalizedEmail = email.ToUpperInvariant();
            FirstName = firstName;
            LastName = lastName;
            PhoneNumber = phoneNumber;
            CreatedAt = createdAt;
            SecurityStamp = Guid.NewGuid().ToString();
        }

        // Domain Methods
        public void UpdateProfile(DateTime createdAt, string firstName, string lastName, string? profileImageUrl = null)
        {
            FirstName = firstName;
            LastName = lastName;
            if (profileImageUrl != null)
                ProfileImageUrl = profileImageUrl;
            UpdatedAt = createdAt;
        }

        public void SetRefreshToken(DateTime createdAt, string refreshToken, int expirationDays)
        {
            var salt = GenerateSalt();
            RefreshTokenSalt = Convert.ToBase64String(salt);
            RefreshTokenHash = HashToken(refreshToken, salt);
            RefreshTokenExpiryTime = createdAt.AddDays(expirationDays);
        }

        public bool ValidateRefreshToken(DateTime createdAt, string refreshToken)
        {
            if (string.IsNullOrEmpty(RefreshTokenHash) ||
                string.IsNullOrEmpty(RefreshTokenSalt) ||
                RefreshTokenExpiryTime <= createdAt)
                return false;

            var salt = Convert.FromBase64String(RefreshTokenSalt);
            var hashedToken = HashToken(refreshToken, salt);
            return RefreshTokenHash == hashedToken;
        }

        public void RevokeRefreshToken()
        {
            RefreshTokenHash = null;
            RefreshTokenSalt = null;
            RefreshTokenExpiryTime = null;
        }

        public void RecordLogin(DateTime lastLoginAt,string? ipAddress = null)
        {
            LastLoginAt = lastLoginAt;
            LastLoginIp = ipAddress;
            FailedLoginAttempts = 0;
        }

        public void RecordFailedLogin(DateTime dateTimeNow)
        {
            FailedLoginAttempts++;
            if (FailedLoginAttempts >= 5)
            {
                LockoutEndDate = dateTimeNow.AddMinutes(15);
            }
        }

        public void Activate() => IsActive = true;
        public void Deactivate() => IsActive = false;

        public void VerifyEmail()
        {
            EmailVerified = true;
            EmailConfirmed = true;
            EmailVerificationToken = null;
            EmailVerificationTokenExpiry = null;
        }

        // Helper Methods
        private static byte[] GenerateSalt()
        {
            var salt = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);
            return salt;
        }

        private static string HashToken(string token, byte[] salt)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(
                token,
                salt,
                10000,
                HashAlgorithmName.SHA256);
            return Convert.ToBase64String(pbkdf2.GetBytes(32));
        }
    }

}
