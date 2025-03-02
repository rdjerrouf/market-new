using Market.Services;
using Market.ViewModels;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;
using System.Diagnostics;

namespace Market.Views
{
    public partial class SetLocationPage : ContentPage
    {
        private readonly SetLocationViewModel _viewModel;
        private Pin _locationPin;

        public SetLocationPage(SetLocationViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            _viewModel = viewModel;

            // Create a pin that we'll manage programmatically
            _locationPin = new Pin
            {
                Label = "Item Location",
                Type = PinType.Place
            };
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                // Get the item ID from query parameter
                var itemIdString = await Shell.Current.GetQueryParameterAsync("ItemId");

                if (!string.IsNullOrEmpty(itemIdString) && int.TryParse(itemIdString, out int itemId))
                {
                    await _viewModel.InitializeAsync(itemId);

                    // If we have a pre-existing location, center the map on it and add a pin
                    if (_viewModel.HasSelectedLocation)
                    {
                        UpdatePinLocation(_viewModel.SelectedLocation);

                        LocationMap.MoveToRegion(
                            MapSpan.FromCenterAndRadius(
                                _viewModel.SelectedLocation,
                                Distance.FromKilometers(1)
                            )
                        );
                    }
                    else
                    {
                        // Otherwise try to center on user's current location
                        var currentLocation = await _viewModel.GetCurrentLocationAsync();
                        if (currentLocation != null)
                        {
                            LocationMap.MoveToRegion(
                                MapSpan.FromCenterAndRadius(
                                    currentLocation,
                                    Distance.FromKilometers(5)
                                )
                            );
                        }
                    }
                }
                else
                {
                    await DisplayAlert("Error", "Invalid item ID", "OK");
                    await Shell.Current.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in SetLocationPage.OnAppearing: {ex.Message}");
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
                await Shell.Current.GoToAsync("..");
            }
        }

        private void OnMapClicked(object sender, MapClickedEventArgs e)
        {
            _viewModel.SetSelectedLocation(e.Location);
            UpdatePinLocation(e.Location);
        }

        // Handle pin location updates
        private void UpdatePinLocation(Location location)
        {
            try
            {
                // Remove existing pin if it exists
                if (LocationMap.Pins.Contains(_locationPin))
                {
                    LocationMap.Pins.Remove(_locationPin);
                }

                // Update pin location
                _locationPin.Location = location;

                // Add pin to map
                LocationMap.Pins.Add(_locationPin);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating pin location: {ex.Message}");
            }
        }

        // Subscribe to location changes from the ViewModel
        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == nameof(_viewModel.SelectedLocation) && _viewModel.HasSelectedLocation)
            {
                UpdatePinLocation(_viewModel.SelectedLocation);
            }
        }
    }
}