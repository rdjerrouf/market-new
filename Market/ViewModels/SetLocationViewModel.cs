using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Market.Services;
using Microsoft.Maui.Devices.Sensors;
using System.Diagnostics;

namespace Market.ViewModels
{
    public partial class SetLocationViewModel : ObservableObject
    {
        private readonly IItemLocationService _itemLocationService;
        private readonly IGeolocationService _geolocationService;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private Location _selectedLocation;

        [ObservableProperty]
        private string _locationName;

        [ObservableProperty]
        private bool _hasSelectedLocation;

        // Use ObservableProperty as normal, the method is now renamed
        [ObservableProperty]
        private string _searchAddress = string.Empty;

        private int _itemId;

        public SetLocationViewModel(IItemLocationService itemLocationService, IGeolocationService geolocationService)
        {
            _itemLocationService = itemLocationService;
            _geolocationService = geolocationService;
            SelectedLocation = new Location();
            LocationName = "No location selected";
            _searchAddress = string.Empty;
        }

        public async Task InitializeAsync(int itemId)
        {
            try
            {
                IsBusy = true;
                _itemId = itemId;

                // Check if the item already has a location
                var itemLocation = await _itemLocationService.GetItemLocationAsync(itemId);
                if (itemLocation != null)
                {
                    SelectedLocation = new Location(itemLocation.Latitude, itemLocation.Longitude);
                    LocationName = itemLocation.LocationName ?? "Location set";
                    HasSelectedLocation = true;
                }
                else
                {
                    HasSelectedLocation = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing location view: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task<Location?> GetCurrentLocationAsync()
        {
            return await _geolocationService.GetCurrentLocation();
        }

        public async void SetSelectedLocation(Location location)
        {
            try
            {
                SelectedLocation = location;
                HasSelectedLocation = true;

                // Update location name by reverse geocoding
                LocationName = "Getting address...";
                var name = await _geolocationService.GetLocationName(location);
                LocationName = name ?? "Unknown location";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error setting location: {ex.Message}");
                LocationName = "Error getting address";
            }
        }

        [RelayCommand]
        private async Task UseCurrentLocation()
        {
            try
            {
                IsBusy = true;

                var location = await _geolocationService.GetCurrentLocation();
                if (location != null)
                {
                    SetSelectedLocation(location);
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Could not determine your current location. Please check your device location settings and permissions.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting current location: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to access location services. Please ensure location permissions are granted.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Renamed method to avoid conflict with the SearchAddress property
        [RelayCommand]
        private async Task SearchAddressCommand()
        {
            if (string.IsNullOrWhiteSpace(SearchAddress))
                return;

            try
            {
                IsBusy = true;

                var location = await _geolocationService.GetLocationFromAddress(SearchAddress);
                if (location != null)
                {
                    SetSelectedLocation(location);
                }
                else
                {
                    await Shell.Current.DisplayAlert("Not Found", "Could not find the specified address. Please try a different search term.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error searching address: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to search for the address.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task SaveLocation()
        {
            if (!HasSelectedLocation)
                return;

            try
            {
                IsBusy = true;

                var success = await _itemLocationService.SaveItemLocationAsync(_itemId, SelectedLocation);
                if (success)
                {
                    await Shell.Current.DisplayAlert("Success", "Location saved successfully!", "OK");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to save location. Please try again.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving location: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "An error occurred while saving the location.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}