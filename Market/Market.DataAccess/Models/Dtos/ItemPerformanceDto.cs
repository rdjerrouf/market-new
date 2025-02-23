using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.DataAccess.Models.Dtos
{
    public class ItemPerformanceDto
    {
        public int ItemId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int ViewCount { get; set; }
        public int InquiryCount { get; set; }
    }
}