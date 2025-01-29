// Models/User.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace Market.DataAccess.Models
{

    /// <summary>
    /// Represents a user in the marketplace system
    /// Stores all user-related information and authentication details
    /// </summary>
    public class User
    {
        /// <summary>
        /// Unique identifier for the user
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// User's email address, used for login and communications
        /// </summary>
        [Required]
        public required string Email { get; set; }

        /// <summary>
        /// Hashed password for user authentication
        /// </summary>
        [Required]
        public required string PasswordHash { get; set; }

        /// <summary>
        /// Optional phone number for contact
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Optional city location
        /// </summary>
        public string? City { get; set; }

        /// <summary>
        /// Optional province/state location
        /// </summary>
        public string? Province { get; set; }

        /// <summary>
        /// When the user account was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}