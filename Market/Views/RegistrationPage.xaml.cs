// RegistrationPage.xaml.cs
using Market.ViewModels;
using Market.Services;

namespace Market.Views
{
    public partial class RegistrationPage : ContentPage
    {
        private readonly RegistrationViewModel _viewModel;

        public RegistrationPage(RegistrationViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.InitializeAsync();
        }
    }
}