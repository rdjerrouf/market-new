using Market.Views;  // Make sure this includes InboxPage
using System.Diagnostics;

namespace Market
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            RegisterRoutes();
            Debug.WriteLine("AppShell initialized");
        }

        private void RegisterRoutes()
        {
            Routing.RegisterRoute("PostItemPage", typeof(PostItemPage));
            Routing.RegisterRoute("InboxPage", typeof(InboxPage)); // Add this line
            Debug.WriteLine("Routes registered");
        }
    }
}