// Models/Message.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace Market.DataAccess.Models
{
    /// <summary>
    /// Represents a message between users
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Unique identifier for the message
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Content of the message
        /// </summary>
        [Required]
        public required string Content { get; set; }

        /// <summary>
        /// ID of sending user
        /// </summary>
        public int SenderId { get; set; }

        /// <summary>
        /// ID of receiving user
        /// </summary>
        public int ReceiverId { get; set; }

        /// <summary>
        /// When the message was sent
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Whether message has been read
        /// </summary>
        public bool IsRead { get; set; } = false;

        /// <summary>
        /// Optional reference to related item
        /// </summary>
        public int? RelatedItemId { get; set; }
    }
}