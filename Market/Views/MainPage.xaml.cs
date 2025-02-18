using Market.ViewModels;
using Market.Services;


namespace Market.Views
{
    public partial class MainPage : ContentPage
    {
        /// <summary>
        /// Initialize MainPage with proper dependency injection
        /// </summary>
        public MainPage(MainViewModel viewModel)  // Change this line
        {
            InitializeComponent();
            BindingContext = viewModel;  // Simply assign the injected viewModel
        }
    }
}