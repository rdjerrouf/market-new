using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Market.DataAccess.Models;

namespace Market.Market.DataAccess.Models
{
    public class ItemStatistics
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public required Item Item { get; set; }
        public int ViewCount { get; set; } = 0;
        public int InquiryCount { get; set; } = 0;
        public int FavoriteCount { get; set; } = 0;
        public DateTime? FirstViewedAt { get; set; }
        public DateTime? LastViewedAt { get; set; }
        public TimeSpan TotalTimeOnMarket { get; set; }
    }
}
