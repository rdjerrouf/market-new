using Market.Services;
using Market.ViewModels;

namespace Market.Views
{
    public partial class UserRatingsPage : ContentPage
    {
        private readonly UserRatingsViewModel _viewModel;

        public UserRatingsPage(UserRatingsViewModel viewModel)
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
                // Get the user ID from query parameter
                var userIdString = await Shell.Current.GetQueryParameterAsync("UserId");

                if (!string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out int userId))
                {
                    await _viewModel.InitializeAsync(userId);
                }
                else
                {
                    await DisplayAlert("Error", "Invalid user ID", "OK");
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