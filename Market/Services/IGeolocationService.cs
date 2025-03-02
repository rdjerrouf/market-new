using Microsoft.Maui.Devices.Sensors;
using Market.DataAccess.Models;

namespace Market.Services
{
    public interface IGeolocationService
    {
        Task<Location?> GetCurrentLocation();
        Task<string?> GetLocationName(Location location);
        Task<Location?> GetLocationFromAddress(string address);
        double CalculateDistance(Location location1, Location location2);
        List<Item> FindItemsWithinRadius(List<Item> items, Location currentLocation, double radiusKm);
        List<Item> SortItemsByDistance(List<Item> items, Location currentLocation);
    }
}