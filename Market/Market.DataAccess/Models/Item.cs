// Models/Item.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace Market.DataAccess.Models
{
    /// <summary>
    /// Represents a marketplace item listing, accommodating various categories such as For Sale, Jobs, Services, and Rentals
    /// </summary>
    public class Item
    {
        // Existing properties
        /// <summary>
        /// Unique identifier for the item
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Title of the item listing
        /// </summary>
        [Required]
        public required string Title { get; set; }

        /// <summary>
        /// Detailed description of the item
        /// </summary>
        [Required]
        public required string Description { get; set; }

        /// <summary>
        /// Optional URL to item's photo
        /// </summary>
        public string? PhotoUrl { get; set; }

        /// <summary>
        /// Price of the item or rate for service/job
        /// </summary>
        [Required]
        [Range(0, 999999.99)] // Changed minimum to 0 to allow free items/services
        public required decimal Price { get; set; } = 0.00M;

        /// <summary>
        /// ID of user who posted the item
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// When the item was listed
        /// </summary>
        public DateTime ListedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Category of the item (e.g., For Sale, Jobs, Services, Rentals)
        /// </summary>
        [Required]
        public required string Category { get; set; } // Changed from Status to Category

        // New properties for different categories
        /// <summary>
        /// Type of job (e.g., Full-time, Part-time) - only for Jobs category
        /// </summary>
        public string? JobType { get; set; }

        /// <summary>
        /// Type of service offered - only for Services category
        /// </summary>
        public string? ServiceType { get; set; }

        /// <summary>
        /// Rental period (e.g., Daily, Weekly, Monthly) - only for Rentals category
        /// </summary>
        public string? RentalPeriod { get; set; }

        /// <summary>
        /// Start date for job or rental availability
        /// </summary>
        public DateTime? AvailableFrom { get; set; }

        /// <summary>
        /// End date for rental availability
        /// </summary>
        public DateTime? AvailableTo { get; set; }
    }
}