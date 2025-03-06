// Models/Report.cs
using Market.DataAccess.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace Market.DataAccess.Models
{
    public class Report
    {
        public int Id { get; set; }

        public int ReportedItemId { get; set; }
        public Item ReportedItem { get; set; } = null!;

        public int ReportedByUserId { get; set; }
        public User ReportedByUser { get; set; } = null!;

        [Required]
        public required string Reason { get; set; }

        public string? AdditionalComments { get; set; }

        public DateTime ReportedAt { get; set; } = DateTime.UtcNow;

        public ReportStatus Status { get; set; } = ReportStatus.Pending;

        public DateTime? ReviewedAt { get; set; }

        public string? ReviewNotes { get; set; }
    }

    public enum ReportStatus
    {
        Pending = 0,
        Reviewed = 1,
        Rejected = 2,
        ActionTaken = 3
    }
}