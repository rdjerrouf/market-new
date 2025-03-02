using Microsoft.Maui.Devices.Sensors;

namespace Market.DataAccess.Models
{
    public class ItemLocation
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? LocationName { get; set; }

        // Navigation property
        public Item? Item { get; set; }

        // Helper method to convert to Location object
        public Location ToLocation()
        {
            return new Location(Latitude, Longitude);
        }

        // Create from a Location object
        public static ItemLocation FromLocation(Location location, int itemId, string? locationName = null)
        {
            return new ItemLocation
            {
                ItemId = itemId,
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                LocationName = locationName
            };
        }
    }
}