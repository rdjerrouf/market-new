using Market.Services;
using Market.ViewModels;

namespace Market.Views
{
    public partial class CreateRatingPage : ContentPage
    {
        private readonly CreateRatingViewModel _viewModel;

        public CreateRatingPage(CreateRatingViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                // Get the item ID and seller ID from query parameters
                var itemIdString = await Shell.Current.GetQueryParameterAsync("ItemId");
                var sellerIdString = await Shell.Current.GetQueryParameterAsync("SellerId");

                if (!string.IsNullOrEmpty(itemIdString) && int.TryParse(itemIdString, out int itemId) &&
                    !string.IsNullOrEmpty(sellerIdString) && int.TryParse(sellerIdString, out int sellerId))
                {
                    await _viewModel.InitializeAsync(itemId, sellerId);
                }
                else
                {
                    await DisplayAlert("Error", "Invalid parameters for rating", "OK");
                    await Shell.Current.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
                await Shell.Current.GoToAsync("..");
            }
        }
    }
}