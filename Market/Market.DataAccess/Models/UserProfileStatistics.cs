using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Market.DataAccess.Models
{
    public class UserProfileStatistics
    {
        public int UserId { get; set; }
        public int PostedItemsCount { get; set; }
        public int FavoriteItemsCount { get; set; }
        public double AverageRating { get; set; }
    }
}
