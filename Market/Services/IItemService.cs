using Market.DataAccess.Models;

namespace Market.Services
{
    // Defines operations available for managing marketplace items
    public interface IItemService
    {
        // Creates a new item listing in the marketplace
        Task<bool> AddItemAsync(Item item);

        // Updates existing item's information
        Task<bool> UpdateItemAsync(Item item);

        // Removes an item from the marketplace
        Task<bool> DeleteItemAsync(int id);

        // Retrieves a specific item by its ID
        Task<Item?> GetItemAsync(int id);

        // Gets all items in the marketplace
        Task<IEnumerable<Item>> GetItemsAsync();

        // Gets all items posted by a specific user
        Task<IEnumerable<Item>> GetUserItemsAsync(int userId);

        // Searches items based on text and optional category
        Task<IEnumerable<Item>> SearchItemsAsync(string searchTerm, string? category = null);

        // adding my listings

        Task<IEnumerable<Item>> GetItemsByUserAsync(int userId);
    }
}