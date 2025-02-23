using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Market.DataAccess.Models; 
using Market.Market.DataAccess.Models;
namespace Market.Market.DataAccess.Models.Dtos
{
    public class CreateItemDto
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public decimal Price { get; set; }
        public required string Category { get; set; }
        public string? JobType { get; set; }
        public string? ServiceType { get; set; }
        public List<string>? PhotoUrls { get; set; }
        public JobCategory? JobCategory { get; set; }
        public string? CompanyName { get; set; }
        public string? JobLocation { get; set; }
        public ApplyMethod? ApplyMethod { get; set; }
        public string? ApplyContact { get; set; }
    }
}
