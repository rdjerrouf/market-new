using Market.ViewModels;

namespace Market.Views
{
    public partial class NearbyItemsPage : ContentPage
    {
        private readonly NearbyItemsViewModel _viewModel;

        public NearbyItemsPage(NearbyItemsViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.InitializeAsync();
        }
    }
}