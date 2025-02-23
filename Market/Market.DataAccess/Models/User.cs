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

        public string? PhoneNumber { get; set; }

        public bool EmailVerified { get; set; }
        public string? City { get; set; }

        public string? Province { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Profile properties
        public string? DisplayName { get; set; }
        public string? ProfilePicture { get; set; }
        public string? Bio { get; set; }

        // Favorite items
        public ICollection<Item> FavoriteItems { get; set; } = new List<Item>();

        // Posted items
        public ICollection<Item> PostedItems { get; set; } = new List<Item>();

        // Privacy settings
        public bool ShowEmail { get; set; } = false;
        public bool ShowPhoneNumber { get; set; } = false;

        public bool IsEmailVerified { get; set; } = false;
        public DateTime? EmailVerifiedAt { get; set; }

    }
}