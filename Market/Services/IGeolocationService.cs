using Microsoft.Maui.Devices.Sensors;

namespace Market.Services
{
    public interface IGeolocationService
    {
        Task<Location?> GetCurrentLocation();
        Task<string?> GetLocationName(Location location);
    }
}