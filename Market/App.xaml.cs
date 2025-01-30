using Market.Views;
using Market.Services;
using Market.ViewModels;
using System.Diagnostics;

namespace Market
{
    public partial class App : Application
    {
        public App()
        {
            Debug.WriteLine("App constructor starting...");
            try
            {
                InitializeComponent();
                Debug.WriteLine("InitializeComponent completed");

                // Create and set AppShell with proper error handling
                var appShell = new AppShell();
                MainPage = appShell;
                Debug.WriteLine("AppShell created and set as MainPage");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in App constructor: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = base.CreateWindow(activationState);

            // Handle window created event for additional setup
            if (window != null)
            {
                window.Created += (s, e) =>
                {
                    Debug.WriteLine("Window created");
                };
            }

            return window;
        }
    }
}