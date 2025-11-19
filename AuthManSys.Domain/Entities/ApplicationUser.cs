using System;
using Microsoft.AspNetCore.Identity;

namespace AuthManSys.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int UserId { get; set; }
        public string EmailConfirmationToken { get; set; }
        public string PasswordResetToken { get; set; }
        public string RequestVerificationToken { get; set; }
        public bool TermsConditionsAccepted { get; set; }
        public DateTime? RequestVerificationTokenExpiration { get; set; }
        public DateTime LastPasswordChangedDate { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }

        // Two-Factor Authentication properties
        public string? TwoFactorCode { get; set; }
        public DateTime? TwoFactorCodeExpiration { get; set; }
        public bool IsTwoFactorEnabled { get; set; } = false;
        public DateTime? TwoFactorCodeGeneratedAt { get; set; }
    }
}