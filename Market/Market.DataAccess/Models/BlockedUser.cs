// Models/BlockedUser.cs
using Market.DataAccess.Models;
using System;

namespace Market.DataAccess.Models
{
    public class BlockedUser
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int BlockedUserId { get; set; }
        public User BlockedUserProfile { get; set; } = null!;

        public DateTime BlockedAt { get; set; } = DateTime.UtcNow;

        public string? Reason { get; set; }
    }
}