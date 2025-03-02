using Market.DataAccess.Data;
using Market.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Devices.Sensors;
using System.Diagnostics;

namespace Market.Services
{
    public class ItemLocationService : IItemLocationService
    {
        private readonly AppDbContext _context;
        private readonly IGeolocationService _geolocationService;

        public ItemLocationService(AppDbContext context, IGeolocationService geolocationService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _geolocationService = geolocationService ?? throw new ArgumentNullException(nameof(geolocationService));
        }

        // Save an item's location
        public async Task<bool> SaveItemLocationAsync(int itemId, Location location)
        {
            try
            {
                var locationName = await _geolocationService.GetLocationName(location);

                var itemLocation = await _context.ItemLocations
                    .FirstOrDefaultAsync(il => il.ItemId == itemId);

                if (itemLocation == null)
                {
                    // Create new location
                    itemLocation = new ItemLocation
                    {
                        ItemId = itemId,
                        Latitude = location.Latitude,
                        Longitude = location.Longitude,
                        LocationName = locationName
                    };

                    _context.ItemLocations.Add(itemLocation);
                }
                else
                {
                    // Update existing location
                    itemLocation.Latitude = location.Latitude;
                    itemLocation.Longitude = location.Longitude;
                    itemLocation.LocationName = locationName;

                    _context.ItemLocations.Update(itemLocation);
                }

                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving item location: {ex.Message}");
                return false;
            }
        }

        // Get an item's location
        public async Task<ItemLocation?> GetItemLocationAsync(int itemId)
        {
            try
            {
                return await _context.ItemLocations
                    .FirstOrDefaultAsync(il => il.ItemId == itemId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving item location: {ex.Message}");
                return null;
            }
        }

        // Delete an item's location
        public async Task<bool> DeleteItemLocationAsync(int itemId)
        {
            try
            {
                var itemLocation = await _context.ItemLocations
                    .FirstOrDefaultAsync(il => il.ItemId == itemId);

                if (itemLocation == null)
                    return false;

                _context.ItemLocations.Remove(itemLocation);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting item location: {ex.Message}");
                return false;
            }
        }

        // Find items within a specific radius of a location
        public async Task<List<Item>> FindItemsNearLocationAsync(Location location, double radiusKm)
        {
            try
            {
                // Get all items with locations
                var itemsWithLocations = await _context.Items
                    .Include(i => i.ItemLocation)
                    .Where(i => i.ItemLocation != null)
                    .ToListAsync();

                // Filter by distance
                return _geolocationService.FindItemsWithinRadius(itemsWithLocations, location, radiusKm);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error finding nearby items: {ex.Message}");
                return new List<Item>();
            }
        }

        // Find items near the user's current location
        public async Task<List<Item>> FindNearbyItemsAsync(double radiusKm)
        {
            try
            {
                var currentLocation = await _geolocationService.GetCurrentLocation();
                if (currentLocation == null)
                    return new List<Item>();

                return await FindItemsNearLocationAsync(currentLocation, radiusKm);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error finding nearby items: {ex.Message}");
                return new List<Item>();
            }
        }

        // Sort items by distance from current location
        public async Task<List<Item>> SortItemsByDistanceAsync(List<Item> items)
        {
            try
            {
                var currentLocation = await _geolocationService.GetCurrentLocation();
                if (currentLocation == null)
                    return items;

                return _geolocationService.SortItemsByDistance(items, currentLocation);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error sorting items by distance: {ex.Message}");
                return items;
            }
        }
    }
}