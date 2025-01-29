using Market.ViewModels;
using Market.DataAccess;
namespace Market.Views
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            BindingContext = new MainViewModel();
        }
        private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
        {
            var viewModel = BindingContext as MainViewModel;
            viewModel?.SearchCommand.Execute(e.NewTextValue);
        }
        private void OnForSaleClicked(object sender, EventArgs e)
        {
            // Set a breakpoint here
            var viewModel = BindingContext as MainViewModel;
            viewModel?.ForSaleCommand.Execute(null);
        }
        private void OnJobsClicked(object sender, EventArgs e)
        {
            // Set a breakpoint here
            var viewModel = BindingContext as MainViewModel;
            viewModel?.JobsCommand.Execute(null);
        }
        private void OnServicesClicked(object sender, EventArgs e)
        {
            // Set a breakpoint here
            var viewModel = BindingContext as MainViewModel;
            viewModel?.ServicesCommand.Execute(null);
        }
        private void OnRentalsClicked(object sender, EventArgs e)
        {
            // Set a breakpoint here
            var viewModel = BindingContext as MainViewModel;
            viewModel?.RentalsCommand.Execute(null);
        }
        private void OnHomeClicked(object sender, EventArgs e)
        {
            // Set a breakpoint here
            var viewModel = BindingContext as MainViewModel;
            viewModel?.HomeCommand.Execute(null);
        }
        private void OnInboxClicked(object sender, EventArgs e)
        {
            // Set a breakpoint here
            var viewModel = BindingContext as MainViewModel;
            viewModel?.InboxCommand.Execute(null);
        }
        private void OnPostClicked(object sender, EventArgs e)
        {
            // Set a breakpoint here
            var viewModel = BindingContext as MainViewModel;
            viewModel?.PostCommand.Execute(null);
        }
        private void OnMyListingsClicked(object sender, EventArgs e)
        {
            // Set a breakpoint here
            var viewModel = BindingContext as MainViewModel;
            viewModel?.MyListingsCommand.Execute(null);
        }
    }
}