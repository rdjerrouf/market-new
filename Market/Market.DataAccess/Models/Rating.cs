using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Market.DataAccess.Models;

namespace Market.Market.DataAccess.Models
{
    public class Rating
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ItemId { get; set; }
        public int Score { get; set; }
        public string? Review { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
        public Item Item { get; set; } = null!;
        public bool IsVerifiedPurchase { get; set; } = false;
        public int? HelpfulVotes { get; set; } = 0;

    }
}
