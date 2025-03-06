using Market.DataAccess.Models;
using Market.Market.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Market.DataAccess.Data;

namespace Market.Services
{
    public class ItemStatusService
    {
        private readonly AppDbContext _dbContext;

        public ItemStatusService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Mark an item as active
        public async Task<bool> MarkAsActiveAsync(int itemId)
        {
            var item = await _dbContext.Items.FindAsync(itemId);
            if (item == null)
                return false;

            item.Status = ItemStatus.Active;
            _dbContext.Update(item);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        // Mark an item as sold
        public async Task<bool> MarkAsSoldAsync(int itemId)
        {
            var item = await _dbContext.Items.FindAsync(itemId);
            if (item == null)
                return false;

            item.Status = ItemStatus.Sold;
            _dbContext.Update(item);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        // Mark an item as rented
        public async Task<bool> MarkAsRentedAsync(int itemId)
        {
            var item = await _dbContext.Items.FindAsync(itemId);
            if (item == null)
                return false;

            item.Status = ItemStatus.Rented;
            _dbContext.Update(item);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        // Mark an item as unavailable
        public async Task<bool> MarkAsUnavailableAsync(int itemId)
        {
            var item = await _dbContext.Items.FindAsync(itemId);
            if (item == null)
                return false;

            item.Status = ItemStatus.Unavailable;
            _dbContext.Update(item);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        // Check if current user is the owner of an item
        public async Task<bool> IsItemOwnerAsync(int itemId, int userId)
        {
            var item = await _dbContext.Items.FindAsync(itemId);
            return item != null && item.PostedByUserId == userId;
        }
    }
}