// SignInPage.xaml.cs
using Market.ViewModels;
using Market.Services;

namespace Market.Views
{
    public partial class SignInPage : ContentPage
    {
        // Initialize field directly to ensure non-null
        private readonly SignInViewModel _viewModel = new SignInViewModel(new AuthService());

        public SignInPage()
        {
            InitializeComponent();
            try
            {
                // Set binding context
                BindingContext = _viewModel;
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", "Failed to initialize: " + ex.Message, "OK");
            }
        }
    }
}