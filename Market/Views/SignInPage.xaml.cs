// SignInPage.xaml.cs
using Market.ViewModels;
using Market.Services;

namespace Market.Views
{
    public partial class SignInPage : ContentPage
    {
        private readonly SignInViewModel _viewModel;

        public SignInPage(SignInViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }
    }
}