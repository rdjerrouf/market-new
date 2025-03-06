using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Market.DataAccess.Models;
using Market.Services;
using Market.Views;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using System.Diagnostics;

namespace Market.ViewModels
{
    public partial class ItemMapViewModel : ObservableObject
    {
        private readonly IItemService _itemService;
        private readonly IItemLocationService _itemLocationService;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _itemTitle;

        [ObservableProperty]
        private string _itemAddress;

        [ObservableProperty]
        private Location _itemLocation;

        [ObservableProperty]
        private bool _hasLocation;

        private int _itemId;
        private Item _displayedItem;

        public ItemMapViewModel(IItemService itemService, IItemLocationService itemLocationService)
        {
            _itemService = itemService;
            _itemLocationService = itemLocationService;
            ItemTitle = string.Empty;
            ItemAddress = string.Empty;
            ItemLocation = new Location();
        }

        public async Task InitializeAsync(int itemId)
        {
            try
            {
                IsBusy = true;
                _itemId = itemId;

                // Get the item
                var item = await _itemService.GetItemAsync(itemId);
                if (item == null)
                {
                    Debug.WriteLine("Item not found");
                    return;
                }

                _displayedItem = item;
                ItemTitle = item.Title;

                // Get location data
                var itemLocation = await _itemLocationService.GetItemLocationAsync(itemId);
                if (itemLocation != null)
                {
                    ItemLocation = new Location(itemLocation.Latitude, itemLocation.Longitude);
                    ItemAddress = itemLocation.LocationName ?? "Location available";
                    HasLocation = true;
                }
                else
                {
                    ItemAddress = "No location data available";
                    HasLocation = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing map view: {ex.Message}");
                ItemAddress = "Error loading location";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task OpenDirections()
        {
            if (!HasLocation)
                return;

            try
            {
                // Open default map app with directions to this location
                var placemark = new Placemark
                {
                    Location = new Location(ItemLocation.Latitude, ItemLocation.Longitude),
                    CountryName = "Canada", // Assuming Canada, modify as needed
                    AdminArea = "Ontario",  // Assuming Ontario, modify as needed
                    Thoroughfare = ItemTitle
                };

                var options = new MapLaunchOptions
                {
                    Name = ItemTitle,
                    NavigationMode = NavigationMode.Driving
                };

                // Use fully qualified name to avoid ambiguity
                await Microsoft.Maui.ApplicationModel.Map.OpenAsync(placemark, options);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error opening map: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Could not open maps application", "OK");
            }
        }

        [RelayCommand]
        private async Task ReportItem()
        {
            // Make sure you have a way to get the current item ID in your map view model
            if (_displayedItem == null || _displayedItem.Id <= 0) return;

            await Shell.Current.GoToAsync($"{nameof(ReportItemPage)}?ItemId={_displayedItem.Id}");
        }
    }
}
