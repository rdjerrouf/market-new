// RegistrationPage.xaml.cs
using Market.ViewModels;
using Market.Services;

namespace Market.Views
{
    public partial class RegistrationPage : ContentPage
    {
        private readonly RegistrationViewModel _viewModel;

        public RegistrationPage()
        {
            InitializeComponent();
            // Create ViewModel with auth service and set as binding context
            _viewModel = new RegistrationViewModel(new AuthService());
            BindingContext = _viewModel;
        }
    }
}