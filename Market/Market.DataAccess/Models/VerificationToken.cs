using System;
using Market.DataAccess.Models;

namespace Market.Market.DataAccess.Models
{
    public class VerificationToken
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public required User User { get; set; }
        public required string Token { get; set; }
        public VerificationType Type { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; } = false;
    }

    public enum VerificationType
    {
        EmailVerification,
        PhoneVerification,
        PasswordReset
    }
}