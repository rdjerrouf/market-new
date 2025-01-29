// Models/Item.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace Market.DataAccess.Models
{
    /// <summary>
    /// Represents a marketplace item listing
    /// </summary>
    public class Item
    {
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
        /// Price of the item
        /// </summary>
        [Required]
        public decimal Price { get; set; }

        /// <summary>
        /// ID of user who posted the item
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// When the item was listed
        /// </summary>
        public DateTime ListedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Current status of the item
        /// </summary>
        public required string Status { get; set; } = "Available";
    }
}
