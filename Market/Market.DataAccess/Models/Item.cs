using System;
using Market.Market.DataAccess.Models;
using System.ComponentModel.DataAnnotations;

namespace Market.DataAccess.Models
{
    public class Item
    {
        public int Id { get; set; }
        [Required]
        public required string Title { get; set; }
        [Required]
        public required string Description { get; set; }
        public string? PhotoUrl { get; set; }
        [Required]
        [Range(0, 999999.99)]
        public required decimal Price { get; set; } = 0.00M;
        public int UserId { get; set; }
        public DateTime ListedDate { get; set; } = DateTime.UtcNow;
        [Required]
        public required string Category { get; set; }
        public string? JobType { get; set; }
        public string? ServiceType { get; set; }
        public string? RentalPeriod { get; set; }
        public DateTime? AvailableFrom { get; set; }
        public DateTime? AvailableTo { get; set; }

        // Job-specific properties
        public JobCategory? JobCategory { get; set; }
        public string? CompanyName { get; set; }
        public string? JobLocation { get; set; }
        public ApplyMethod? ApplyMethod { get; set; }
        public string? ApplyContact { get; set; }

        // Service-specific properties
        public ServiceCategory? ServiceCategory { get; set; }
        public ServiceAvailability? ServiceAvailability { get; set; }
        public int? YearsOfExperience { get; set; }
        public int? NumberOfEmployees { get; set; }
        public string? ServiceLocation { get; set; }
        public double? AverageRating { get; set; }

        // Add to your existing Item.cs
        public AlState? State { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        // category-specific properties for sale and rent
        public ForSaleCategory? ForSaleCategory { get; set; }
        public ForRentCategory? ForRentCategory { get; set; }
    }
}
