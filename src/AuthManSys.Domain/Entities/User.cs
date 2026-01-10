using System;
using AuthManSys.Domain.Enums;

namespace AuthManSys.Domain.Entities;

public class User
{
        // Identity Framework properties
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string NormalizedUserName { get; set; } = string.Empty;
        public string NormalizedEmail { get; set; } = string.Empty;

        // Domain properties
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public UserStatus Status { get; set; }
        public bool TermsAccepted { get; set; }

        // Authentication properties
        public bool EmailConfirmed { get; set; } = false;
        public string? SecurityStamp { get; set; }
        public string? ConcurrencyStamp { get; set; }

        // Two-Factor Authentication properties
        public bool IsTwoFactorEnabled { get; set; } = false;
        public string? TwoFactorCode { get; set; }
        public DateTime? TwoFactorCodeExpiration { get; set; }

        // Google OAuth properties
        public string? GoogleId { get; set; }
        public string? GoogleEmail { get; set; }
        public string? GooglePictureUrl { get; set; }
        public bool IsGoogleAccount { get; set; } = false;
        public DateTime? GoogleLinkedAt { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        protected User() { } // For ORMs / mapping

        protected User(
            int userId,
            string email,
            string firstName,
            string lastName)
        {
            UserId = userId;
            Email = email;
            FirstName = firstName;
            LastName = lastName;
            Status = UserStatus.Active;
            CreatedAt = DateTime.UtcNow;
        }

        // -------- Business Rules --------

        public void ChangeEmail(string newEmail)
        {
            //ensure that deleted users cannot change email. must be active users only
            if (Status == UserStatus.Deleted)
                throw new Exception("Deleted user cannot change email.");

            Email = newEmail;
        }

        public void AcceptTerms()
        {
            TermsAccepted = true;
        }

        public void MarkLastLogin()
        {
            LastLoginAt = DateTime.UtcNow;
        }


        public void Delete()
        {
            Status = UserStatus.Deleted;
        }

        public string FullName => $"{FirstName} {LastName}";
}
