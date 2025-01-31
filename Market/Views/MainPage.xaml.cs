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
            // Create and set ViewModel with required service
            BindingContext = new MainViewModel(itemService);
        }
    }
}