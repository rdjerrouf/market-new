// Services/SecurityService.cs
using Market.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Market.DataAccess.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Market.Services
{
    public class SecurityService
    {
        private readonly AppDbContext _dbContext;

        public SecurityService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Reporting functionality
        public async Task<Report> ReportItemAsync(int itemId, int reportedByUserId, string reason, string? additionalComments = null)
        {
            // Check if the item exists
            var item = await _dbContext.Items.FindAsync(itemId);
            if (item == null)
                throw new Exception("Item not found");

            // Check if the user exists
            var user = await _dbContext.Users.FindAsync(reportedByUserId);
            if (user == null)
                throw new Exception("User not found");

            // Create the report
            var report = new Report
            {
                ReportedItemId = itemId,
                ReportedByUserId = reportedByUserId,
                Reason = reason,
                AdditionalComments = additionalComments,
                ReportedAt = DateTime.UtcNow,
                Status = ReportStatus.Pending
            };

            // Add to database
            await _dbContext.Reports.AddAsync(report);
            await _dbContext.SaveChangesAsync();

            return report;
        }

        public async Task<List<Report>> GetUserReportsAsync(int userId)
        {
            return await _dbContext.Reports
                .Where(r => r.ReportedByUserId == userId)
                .OrderByDescending(r => r.ReportedAt)
                .ToListAsync();
        }

        public async Task<bool> HasUserReportedItemAsync(int userId, int itemId)
        {
            return await _dbContext.Reports
                .AnyAsync(r => r.ReportedByUserId == userId && r.ReportedItemId == itemId);
        }

        // Blocking functionality
        public async Task<BlockedUser> BlockUserAsync(int userId, int blockedUserId, string? reason = null)
        {
            // Check if the users exist
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
                throw new Exception("User not found");

            var blockedUser = await _dbContext.Users.FindAsync(blockedUserId);
            if (blockedUser == null)
                throw new Exception("User to block not found");

            // Prevent blocking yourself
            if (userId == blockedUserId)
                throw new Exception("You cannot block yourself");

            // Check if already blocked
            var existing = await _dbContext.BlockedUsers
                .FirstOrDefaultAsync(b => b.UserId == userId && b.BlockedUserId == blockedUserId);

            if (existing != null)
                return existing; // Already blocked

            // Create the block record
            var block = new BlockedUser
            {
                UserId = userId,
                BlockedUserId = blockedUserId,
                BlockedAt = DateTime.UtcNow,
                Reason = reason
            };

            // Add to database
            await _dbContext.BlockedUsers.AddAsync(block);
            await _dbContext.SaveChangesAsync();

            return block;
        }

        public async Task<bool> UnblockUserAsync(int userId, int blockedUserId)
        {
            var block = await _dbContext.BlockedUsers
                .FirstOrDefaultAsync(b => b.UserId == userId && b.BlockedUserId == blockedUserId);

            if (block == null)
                return false; // Not blocked

            _dbContext.BlockedUsers.Remove(block);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsUserBlockedAsync(int userId, int blockedUserId)
        {
            return await _dbContext.BlockedUsers
                .AnyAsync(b => b.UserId == userId && b.BlockedUserId == blockedUserId);
        }

        public async Task<List<User>> GetBlockedUsersAsync(int userId)
        {
            return await _dbContext.BlockedUsers
                .Where(b => b.UserId == userId)
                .Include(b => b.BlockedUserProfile)
                .Select(b => b.BlockedUserProfile)
                .ToListAsync();
        }
    }
}