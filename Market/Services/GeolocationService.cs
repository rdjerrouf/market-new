using Microsoft.Maui.Devices.Sensors;
using System.Diagnostics;
using Market.DataAccess.Models;

namespace Market.Services
{
    public class GeolocationService : IGeolocationService
    {
        // Get the current device location
        public async Task<Location?> GetCurrentLocation()
        {
            try
            {
                var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    Debug.WriteLine("Location permission not granted");
                    return null;
                }

                var request = new GeolocationRequest(GeolocationAccuracy.Medium);
                return await Geolocation.GetLocationAsync(request);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to get location: {ex.Message}");
                return null;
            }
        }

        // Get a readable name for a location (reverse geocoding)
        public async Task<string?> GetLocationName(Location location)
        {
            try
            {
                var placemarks = await Geocoding.GetPlacemarksAsync(location.Latitude, location.Longitude);
                var placemark = placemarks?.FirstOrDefault();
                if (placemark != null)
                {
                    // Return city and state/province
                    return $"{placemark.Locality}, {placemark.AdminArea}";
                }
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to get location name: {ex.Message}");
                return null;
            }
        }

        // Get coordinates from an address (forward geocoding)
        public async Task<Location?> GetLocationFromAddress(string address)
        {
            try
            {
                var locations = await Geocoding.GetLocationsAsync(address);
                return locations?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to get location from address: {ex.Message}");
                return null;
            }
        }

        // Calculate distance between two locations in kilometers
        public double CalculateDistance(Location location1, Location location2)
        {
            return Location.CalculateDistance(location1, location2, DistanceUnits.Kilometers);
        }

        // Find items within a specific radius
        public List<Item> FindItemsWithinRadius(List<Item> items, Location currentLocation, double radiusKm)
        {
            return items.Where(item =>
                item.ItemLocation != null &&
                CalculateDistance(currentLocation, item.ItemLocation.ToLocation()) <= radiusKm
            ).ToList();
        }

        // Sort items by distance from current location
        public List<Item> SortItemsByDistance(List<Item> items, Location currentLocation)
        {
            return items
                .Where(item => item.ItemLocation != null)
                .OrderBy(item => CalculateDistance(currentLocation, item.ItemLocation.ToLocation()))
                .ToList();
        }
    }
}