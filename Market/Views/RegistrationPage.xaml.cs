// RegistrationPage.xaml.cs
using Market.ViewModels;
using Market.Services;

namespace Market.Views
{
    public partial class RegistrationPage : ContentPage
    {
        public RegistrationPage(RegistrationViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}