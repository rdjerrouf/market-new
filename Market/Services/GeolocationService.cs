using Microsoft.Maui.Devices.Sensors;
using System.Diagnostics;

namespace Market.Services
{
    public class GeolocationService : IGeolocationService
    {
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
    }
}