﻿using Market.Views;
using Market.Views.AddItem;
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
            try
            {
                // Register existing routes
                Routing.RegisterRoute(nameof(ServiceItemPage), typeof(ServiceItemPage));
                Routing.RegisterRoute(nameof(AddItemPage), typeof(AddItemPage));
                Routing.RegisterRoute(nameof(RentalItemPage), typeof(RentalItemPage));
                Routing.RegisterRoute(nameof(JobItemPage), typeof(JobItemPage));
                Routing.RegisterRoute(nameof(ForSaleItemPage), typeof(ForSaleItemPage));
                Routing.RegisterRoute(nameof(PostItemPage), typeof(PostItemPage));
                Routing.RegisterRoute(nameof(InboxPage), typeof(InboxPage));
                Routing.RegisterRoute(nameof(MyListingsPage), typeof(MyListingsPage));
                Routing.RegisterRoute(nameof(SignInPage), typeof(SignInPage));
                Routing.RegisterRoute(nameof(ItemDetailPage), typeof(ItemDetailPage));
                // Log registered routes
                Debug.WriteLine("Successfully registered routes:");
                Debug.WriteLine($"- {nameof(ServiceItemPage)}");
                Debug.WriteLine($"- {nameof(RentalItemPage)}");
                Debug.WriteLine($"- {nameof(JobItemPage)}");
                Debug.WriteLine($"- {nameof(ForSaleItemPage)}");
                Debug.WriteLine($"- {nameof(PostItemPage)}");
                Debug.WriteLine($"- {nameof(InboxPage)}");
                Debug.WriteLine($"- {nameof(MyListingsPage)}");
                Debug.WriteLine($"- {nameof(SignInPage)}");
                Debug.WriteLine($"- {nameof(ItemDetailPage)}");
            }

            catch (Exception ex)
            {
                Debug.WriteLine($"Error registering routes: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        protected override void OnNavigating(ShellNavigatingEventArgs args)
        {
            base.OnNavigating(args);
            Debug.WriteLine($"Navigating to: {args.Target.Location}");
        }

        protected override void OnNavigated(ShellNavigatedEventArgs args)
        {
            base.OnNavigated(args);
            Debug.WriteLine($"Navigated to: {args.Current.Location}");
        }
    }
}