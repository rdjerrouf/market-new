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
            Routing.RegisterRoute(nameof(PostItemPage), typeof(PostItemPage));
            Routing.RegisterRoute(nameof(InboxPage), typeof(InboxPage));
            Debug.WriteLine($"Routes registered: {nameof(PostItemPage)}, {nameof(InboxPage)}");
            Console.WriteLine("Routes registered");
        }
    }
}