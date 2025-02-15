﻿using Market.Views;
using Market.Services;
using Market.ViewModels;
using System.Diagnostics;
using System.Globalization;


namespace Market
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
            Debug.WriteLine("App constructor called");
            Console.WriteLine("App constructor called");
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Debug.WriteLine($"Unhandled exception: {e.ExceptionObject}");
                Console.WriteLine($"Unhandled exception: {e.ExceptionObject}");
            };
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = base.CreateWindow(activationState);

            if (window != null)
            {
                // Configure window settings
                window.Title = "Market";
                window.MinimumWidth = 400;
                window.MinimumHeight = 600;
            }

            return window ?? throw new InvalidOperationException("Failed to create application window");
        }
    }
}