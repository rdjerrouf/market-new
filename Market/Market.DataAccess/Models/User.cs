// Models/User.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace Market.DataAccess.Models
{

    public class User
    {
        public int Id { get; set; }

        [Required]
        public required string Email { get; set; }

        [Required]
        public required string PasswordHash { get; set; }

        public string?DisplayName { get; set; }
        public string? ProfilePicture { get; set; }
        public string? Bio { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? PhoneNumber { get; set; }
        public string? City { get; set; }
        public string? Province { get; set; }
        public bool ShowEmail { get; set; }
        public bool ShowPhoneNumber { get; set; }

        // Missing properties referenced in code
        public bool IsEmailVerified { get; set; }
        public DateTime? EmailVerifiedAt { get; set; }

        // Navigation properties
        public ICollection<Item> PostedItems { get; set; } = new List<Item>();
        public ICollection<Item> FavoriteItems { get; set; } = new List<Item>();

    }
}