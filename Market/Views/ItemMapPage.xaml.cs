using Market.Services;
using Market.ViewModels;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using System.Diagnostics;

namespace Market.Views
{
    public partial class ItemMapPage : ContentPage
    {
        private readonly ItemMapViewModel _viewModel;
        private Pin _itemPin;

        public ItemMapPage(ItemMapViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            _viewModel = viewModel;

            // Create a pin that we'll manage programmatically
            _itemPin = new Pin
            {
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

                    // If we have a location, center the map on it and add a pin
                    if (_viewModel.HasLocation)
                    {
                        // Update pin
                        _itemPin.Label = _viewModel.ItemTitle;
                        _itemPin.Address = _viewModel.ItemAddress;
                        _itemPin.Location = _viewModel.ItemLocation;

                        // Add pin to map if not already added
                        if (!LocationMap.Pins.Contains(_itemPin))
                        {
                            LocationMap.Pins.Add(_itemPin);
                        }

                        // Center map on location
                        LocationMap.MoveToRegion(
                            MapSpan.FromCenterAndRadius(
                                _viewModel.ItemLocation,
                                Distance.FromKilometers(1)
                            )
                        );
                    }
                    else
                    {
                        // No location, so don't display a pin
                        if (LocationMap.Pins.Contains(_itemPin))
                        {
                            LocationMap.Pins.Remove(_itemPin);
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
                Debug.WriteLine($"Error in ItemMapPage.OnAppearing: {ex.Message}");
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
                await Shell.Current.GoToAsync("..");
            }
        }
    }
}