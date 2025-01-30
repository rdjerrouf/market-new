using Market.DataAccess.Data;
using Market.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Market.Services
{
    // Handles all database operations related to marketplace items
    public class ItemService : IItemService
    {
        private readonly AppDbContext _context;

        public ItemService(AppDbContext context)
        {
            _context = context;
        }

        // Adds a new item to the marketplace
        // Returns true if successful, false if failed
        public async Task<bool> AddItemAsync(Item item)
        {
            try
            {
                await _context.Items.AddAsync(item);
                var result = await _context.SaveChangesAsync();
                return result > 0;  // Returns true if at least one row was affected
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding item: {ex.Message}");
                return false;
            }
        }

        // Updates an existing item's information
        // Returns true if update was successful
        public async Task<bool> UpdateItemAsync(Item item)
        {
            try
            {
                _context.Items.Update(item);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating item: {ex.Message}");
                return false;
            }
        }

        // Removes an item from the marketplace by its ID
        // Returns true if deletion was successful
        public async Task<bool> DeleteItemAsync(int id)
        {
            try
            {
                var item = await _context.Items.FindAsync(id);
                if (item == null) return false;

                _context.Items.Remove(item);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting item: {ex.Message}");
                return false;
            }
        }

        // Retrieves a single item by its ID
        // Returns null if item doesn't exist
        public async Task<Item?> GetItemAsync(int id)
        {
            return await _context.Items.FindAsync(id);
        }

        // Retrieves all items from the marketplace
        // Orders them by most recently listed first
        public async Task<IEnumerable<Item>> GetItemsAsync()
        {
            return await _context.Items
                .OrderByDescending(i => i.ListedDate)
                .ToListAsync();
        }

        // Gets all items posted by a specific user
        // Orders them by most recently listed first
        public async Task<IEnumerable<Item>> GetUserItemsAsync(int userId)
        {
            return await _context.Items
                .Where(i => i.UserId == userId)
                .OrderByDescending(i => i.ListedDate)
                .ToListAsync();
        }

        // Searches items based on search term and optional category
        // searchTerm: Matches against title and description
        // category: Optional filter for item category (For Sale, Jobs, etc.)
        public async Task<IEnumerable<Item>> SearchItemsAsync(string searchTerm, string? category = null)
        {
            // Start with all items
            var query = _context.Items.AsQueryable();

            // Apply search term filter if provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(i =>
                    i.Title.ToLower().Contains(searchTerm) ||
                    i.Description.ToLower().Contains(searchTerm));
            }

            // Apply category filter if provided
            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(i => i.Status == category);
            }

            // Return results ordered by most recent first
            return await query
                .OrderByDescending(i => i.ListedDate)
                .ToListAsync();
        }
    }
}