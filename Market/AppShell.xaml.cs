using Market.Views;  // Add this for PostItemPage
using System.Diagnostics;

namespace Market
{
    /// <summary>
    /// Main shell navigation container for the application
    /// </summary>
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            RegisterRoutes();
            Debug.WriteLine("AppShell initialized");
        }

        /// <summary>
        /// Register navigation routes for the application
        /// </summary>
        private void RegisterRoutes()
        {
            Routing.RegisterRoute("PostItemPage", typeof(PostItemPage));
            Debug.WriteLine("Routes registered");
        }
    }
}