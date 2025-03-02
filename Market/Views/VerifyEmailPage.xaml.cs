using Market.Services;
using Market.ViewModels;

namespace Market.Views
{
    public partial class VerifyEmailPage : ContentPage
    {
        private readonly VerifyEmailViewModel _viewModel;

        public VerifyEmailPage(VerifyEmailViewModel viewModel)
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
                // Get user ID from query parameter
                var userId = await Shell.Current.GetQueryParameterAsync("userId");
                var email = await Shell.Current.GetQueryParameterAsync("email");

                if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out int userIdInt) && !string.IsNullOrEmpty(email))
                {
                    await _viewModel.InitializeAsync(userIdInt, email);
                }
                else
                {
                    await DisplayAlert("Error", "Invalid verification parameters", "OK");
                    await Shell.Current.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
                await Shell.Current.GoToAsync("..");
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _viewModel.Cleanup();
        }
    }
}