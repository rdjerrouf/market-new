// Views/ReportItemPage.xaml.cs
using Market.ViewModels;

namespace Market.Views
{
    public partial class ReportItemPage : ContentPage
    {
        private readonly ReportItemViewModel _viewModel;

        public ReportItemPage(ReportItemViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.InitializeAsync();
        }
    }
}