using System;
using Market.Market.DataAccess.Models;

namespace Market.DataAccess.Models.Dtos
{
    /// <summary>
    /// Data Transfer Object for updating an existing item
    /// </summary>
    public class ItemUpdateDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? Category { get; set; }

        // Optional fields based on item type
        public string? JobType { get; set; }
        public string? ServiceType { get; set; }
        public string? RentalPeriod { get; set; }

        // Location and availability
        public DateTime? AvailableFrom { get; set; }
        public DateTime? AvailableTo { get; set; }

        // Job-specific fields
        public JobCategory? JobCategory { get; set; }
        public string? CompanyName { get; set; }
        public string? JobLocation { get; set; }
        public ApplyMethod? ApplyMethod { get; set; }
        public string? ApplyContact { get; set; }

        // Service-specific fields
        public ServiceCategory? ServiceCategory { get; set; }
        public ServiceAvailability? ServiceAvailability { get; set; }
        public int? YearsOfExperience { get; set; }
        public string? ServiceLocation { get; set; }

        // Sale and Rent fields
        public ForSaleCategory? ForSaleCategory { get; set; }
        public ForRentCategory? ForRentCategory { get; set; }

        // Additional metadata
        public AlState? State { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}