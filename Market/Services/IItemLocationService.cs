using Market.DataAccess.Models;
using Microsoft.Maui.Devices.Sensors;

namespace Market.Services
{
    public interface IItemLocationService
    {
        Task<bool> SaveItemLocationAsync(int itemId, Location location);
        Task<ItemLocation?> GetItemLocationAsync(int itemId);
        Task<bool> DeleteItemLocationAsync(int itemId);
        Task<List<Item>> FindItemsNearLocationAsync(Location location, double radiusKm);
        Task<List<Item>> FindNearbyItemsAsync(double radiusKm);
        Task<List<Item>> SortItemsByDistanceAsync(List<Item> items);
    }
}
