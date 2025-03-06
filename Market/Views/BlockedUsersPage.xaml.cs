// Views/BlockedUsersPage.xaml.cs
using CommunityToolkit.Mvvm.Input;
using Market.ViewModels;

namespace Market.Views
{
    public partial class BlockedUsersPage : ContentPage
    {
        private readonly BlockedUsersViewModel _viewModel;

        public BlockedUsersPage(BlockedUsersViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadBlockedUsersAsync();
        }

        [RelayCommand]
        private async Task ManageBlockedUsers()
        {
            await Shell.Current.GoToAsync($"{nameof(BlockedUsersPage)}");
        }
    }
}