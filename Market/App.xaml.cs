using Market.Views;
using Market.Services;
using Market.ViewModels;
using System.Diagnostics;

namespace Market
{
    // Main application class responsible for app initialization
    public partial class App : Application
    {
        public App()
        {
            Debug.WriteLine("App constructor starting...");

            try
            {
                // Initialize XAML components and resources
                InitializeComponent();
                Debug.WriteLine("InitializeComponent completed");

                // Set the main page to our Shell for navigation
                MainPage = new AppShell();
                Debug.WriteLine("AppShell created and set as MainPage");
            }
            catch (Exception ex)
            {
                // Log any initialization errors for debugging
                Debug.WriteLine($"Error in App constructor: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}