using Market.Views;
using System.Diagnostics;

namespace Market
{
    public partial class AppShell : Shell
    {
        private bool _isNavigating = false;

        public AppShell()
        {
            try
            {
                Debug.WriteLine("Initializing AppShell...");
                InitializeComponent();

                // Register routes for navigation between pages
                RegisterRoutes();

                // Subscribe to loaded event for initial navigation
                Loaded += OnShellLoaded;

                Debug.WriteLine("AppShell initialization completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in AppShell constructor: {ex.Message}");
            }
        }

        private void RegisterRoutes()
        {
            Debug.WriteLine("Registering navigation routes...");
            try
            {
                Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
                Routing.RegisterRoute(nameof(SignInPage), typeof(SignInPage));
                Routing.RegisterRoute(nameof(RegistrationPage), typeof(RegistrationPage));
                Routing.RegisterRoute(nameof(PostItemPage), typeof(PostItemPage));
                Debug.WriteLine("Route registration completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error registering routes: {ex.Message}");
            }
        }

        private async void OnShellLoaded(object sender, EventArgs e)
        {
            try
            {
                // Prevent multiple navigation attempts
                if (_isNavigating)
                    return;

                _isNavigating = true;

                // Remove the event handler
                Loaded -= OnShellLoaded;

                Debug.WriteLine("Shell loaded, attempting navigation to SignInPage...");

                // Use dispatcher to ensure we're on the main thread
                await Dispatcher.DispatchAsync(async () =>
                {
                    try
                    {
                        await GoToAsync("//SignInPage");
                        Debug.WriteLine("Navigation to SignInPage completed successfully");
                    }
                    catch (Exception navEx)
                    {
                        Debug.WriteLine($"Error during navigation: {navEx.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnShellLoaded: {ex.Message}");
            }
            finally
            {
                _isNavigating = false;
            }
        }
    }
}