using Market.Views;
using System.Diagnostics;

namespace Market
{
    // Shell class that handles the app's navigation structure
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            try
            {
                Debug.WriteLine("Initializing AppShell...");
                InitializeComponent();

                // Register routes for navigation between pages
                RegisterRoutes();

                // Navigate to initial page (SignIn for authentication)
                NavigateToInitialPage();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in AppShell constructor: {ex.Message}");
            }
        }

        // Registers all navigation routes used in the app
        private void RegisterRoutes()
        {
            Debug.WriteLine("Registering navigation routes...");

            // Register main pages for navigation
            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
            Routing.RegisterRoute(nameof(SignInPage), typeof(SignInPage));
            Routing.RegisterRoute(nameof(RegistrationPage), typeof(RegistrationPage));
            Routing.RegisterRoute(nameof(PostItemPage), typeof(PostItemPage));

            Debug.WriteLine("Route registration completed");
        }

        // Sets the initial page for the application
        private async void NavigateToInitialPage()
        {
            Debug.WriteLine("Navigating to initial page (SignInPage)...");

            try
            {
                // Navigate to SignIn page as the starting point
                await Current.GoToAsync("//SignInPage");
                Debug.WriteLine("Navigation to SignInPage completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation error: {ex.Message}");
            }
        }
    }
}