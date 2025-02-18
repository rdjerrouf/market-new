using Market.DataAccess.Data;
using Market.DataAccess.Models;
using Market.Market.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Market.Services
{
    /// <summary>
    /// Service for managing marketplace items
    /// </summary>
    public class ItemService : IItemService
    {
        private readonly AppDbContext _context;

        public ItemService(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Adds a new item to the marketplace
        /// </summary>
        public async Task<bool> AddItemAsync(Item item)
        {
            try
            {
                await _context.Items.AddAsync(item);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding item: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Updates an existing item's information
        /// </summary>
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

        /// <summary>
        /// Deletes an item from the marketplace
        /// </summary>
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

        /// <summary>
        /// Retrieves a specific item by its ID
        /// </summary>
        public async Task<Item?> GetItemAsync(int id)
        {
            try
            {
                return await _context.Items.FindAsync(id);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving item: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Retrieves all items in the marketplace
        /// </summary>
        public async Task<IEnumerable<Item>> GetItemsAsync()
        {
            try
            {
                return await _context.Items
                    .OrderByDescending(i => i.ListedDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving items: {ex.Message}");
                return Enumerable.Empty<Item>();
            }
        }

        /// <summary>
        /// Retrieves items posted by a specific user
        /// </summary>
        public async Task<IEnumerable<Item>> GetUserItemsAsync(int userId)
        {
            try
            {
                return await _context.Items
                    .Where(i => i.UserId == userId)
                    .OrderByDescending(i => i.ListedDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving user items: {ex.Message}");
                return Enumerable.Empty<Item>();
            }
        }

        /// <summary>
        /// Searches items based on search term and optional category
        /// </summary>
        public async Task<IEnumerable<Item>> SearchItemsAsync(string searchTerm, string? category = null)
        {
            try
            {
                var query = _context.Items.AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    query = query.Where(i =>
                        i.Title.ToLower().Contains(searchTerm) ||
                        i.Description.ToLower().Contains(searchTerm));
                }

                if (!string.IsNullOrWhiteSpace(category))
                {
                    query = query.Where(i => i.Category == category);
                }

                return await query
                    .OrderByDescending(i => i.ListedDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error searching items: {ex.Message}");
                return Enumerable.Empty<Item>();
            }
        }

        /// <summary>
        /// Alias for GetUserItemsAsync to match interface
        /// </summary>
        public Task<IEnumerable<Item>> GetItemsByUserAsync(int userId) =>
            GetUserItemsAsync(userId);

        public async Task<IEnumerable<Item>> SearchByStateAsync(AlState state)
        {
            try
            {
                return await _context.Items
                    .Where(i => i.State == state)
                    .OrderByDescending(i => i.ListedDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error searching items by state: {ex.Message}");
                return Enumerable.Empty<Item>();
            }
        }

        public async Task<IEnumerable<Item>> SearchByLocationAsync(double latitude, double longitude, double radiusKm)
        {
            try
            {
                // Convert to radians
                var lat1 = latitude * Math.PI / 180;
                var lon1 = longitude * Math.PI / 180;

                return await _context.Items
                    .Where(i => i.Latitude != null && i.Longitude != null)
                    .Select(i => new
                    {
                        Item = i,
                        Distance = 6371 * Math.Acos(
                            Math.Sin(lat1) * Math.Sin(i.Latitude!.Value * Math.PI / 180) +
                            Math.Cos(lat1) * Math.Cos(i.Latitude!.Value * Math.PI / 180) *
                            Math.Cos((i.Longitude!.Value * Math.PI / 180) - lon1)
                        )
                    })
                    .Where(x => x.Distance <= radiusKm)
                    .OrderBy(x => x.Distance)
                    .Select(x => x.Item)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error searching items by location: {ex.Message}");
                return Enumerable.Empty<Item>();
            }
        }

        public async Task<IEnumerable<Item>> SearchByCategoryAndStateAsync(string category, AlState state)
        {
            try
            {
                return await _context.Items
                    .Where(i => i.Category == category && i.State == state)
                    .OrderByDescending(i => i.ListedDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error searching items by category and state: {ex.Message}");
                return Enumerable.Empty<Item>();
            }
        }
    }

}




