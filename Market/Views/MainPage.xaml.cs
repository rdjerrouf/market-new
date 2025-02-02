using Market.ViewModels;
using Market.Services;

namespace Market.Views
{
    public partial class MainPage : ContentPage
    {
        /// <summary>
        /// Initialize MainPage with proper dependency injection
        /// </summary>
        public MainPage(IItemService itemService)
        {
            InitializeComponent();
            // Create ViewModel with the injected ItemService
            var viewModel = new MainViewModel(itemService);
            BindingContext = viewModel;
        }
    }
}