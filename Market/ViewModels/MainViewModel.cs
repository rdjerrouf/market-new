using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using Market.DataAccess.Models;
using System.Diagnostics;

namespace Market.ViewModels
{
    /// <summary>
    /// ViewModel for the main marketplace page.
    /// Handles item listing, searching, and category filtering.
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        // Collection of items displayed in the marketplace
        public ObservableCollection<Item> Items { get; }

        // Search query binding property
        [ObservableProperty]
        private string? searchQuery;

        // Command for search functionality
        public IRelayCommand<string?> SearchCommand { get; }

        /// <summary>
        /// Initializes the main view model and loads initial data
        /// </summary>
        public MainViewModel()
        {
            // Initialize items collection with sample data
            Items = new ObservableCollection<Item>(GetAllItems());

            // Initialize search command
            SearchCommand = new RelayCommand<string?>(SearchItems);

            Debug.WriteLine("MainViewModel initialized");
        }

        /// <summary>
        /// Filters items based on search query
        /// Matches against item title and description
        /// </summary>
        private void SearchItems(string? query)
        {
            Debug.WriteLine($"Searching items with query: {query}");

            try
            {
                Items.Clear();

                if (string.IsNullOrWhiteSpace(query))
                {
                    // Show all items if no search query
                    foreach (var item in GetAllItems())
                    {
                        Items.Add(item);
                    }
                }
                else
                {
                    // Filter items based on query
                    var filteredItems = GetAllItems().Where(item =>
                        item.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        item.Description.Contains(query, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    foreach (var item in filteredItems)
                    {
                        Items.Add(item);
                    }
                }

                Debug.WriteLine($"Search completed. Found {Items.Count} items");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during search: {ex.Message}");
            }
        }

        /// <summary>
        /// Temporary method to get sample items
        /// TODO: Replace with actual database calls
        /// </summary>
        private List<Item> GetAllItems()
        {
            // Sample data for testing
            // Will be replaced with actual database queries
            return new List<Item>
            {
                new Item
                {
                    Id = 1,
                    Title = "Sample Laptop",
                    Description = "Slightly used laptop in great condition",
                    PhotoUrl = "http://example.com/photo1.jpg",
                    Price = 599.99m,
                    UserId = 123,
                    ListedDate = DateTime.UtcNow,
                    Status = "For Sale"
                },
                new Item
                {
                    Id = 2,
                    Title = "Garden Services",
                    Description = "Professional garden maintenance",
                    PhotoUrl = "http://example.com/photo2.jpg",
                    Price = 50.00m,
                    UserId = 124,
                    ListedDate = DateTime.UtcNow,
                    Status = "Services"
                },
                new Item
                {
                    Id = 3,
                    Title = "Room for Rent",
                    Description = "Furnished room in quiet neighborhood",
                    PhotoUrl = "http://example.com/photo3.jpg",
                    Price = 800.00m,
                    UserId = 125,
                    ListedDate = DateTime.UtcNow,
                    Status = "Rentals"
                }
            };
        }

        // Category filter commands
        [RelayCommand]
        private void ForSale()
        {
            Debug.WriteLine("ForSale filter selected");
            // TODO: Implement For Sale category filtering
        }

        [RelayCommand]
        private void Jobs()
        {
            Debug.WriteLine("Jobs filter selected");
            // TODO: Implement Jobs category filtering
        }

        [RelayCommand]
        private void Services()
        {
            Debug.WriteLine("Services filter selected");
            // TODO: Implement Services category filtering
        }

        [RelayCommand]
        private void Rentals()
        {
            Debug.WriteLine("Rentals filter selected");
            // TODO: Implement Rentals category filtering
        }

        // Navigation commands
        [RelayCommand]
        private void Home()
        {
            Debug.WriteLine("Navigating to Home");
            // TODO: Implement Home navigation
        }

        [RelayCommand]
        private void Inbox()
        {
            Debug.WriteLine("Navigating to Inbox");
            // TODO: Implement Inbox navigation
        }

        [RelayCommand]
        private async Task PostAsync()
        {
            try
            {
                Debug.WriteLine("Navigating to PostItemPage...");
                await Shell.Current.GoToAsync("PostItemPage");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error navigating to PostItemPage: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                await Shell.Current.DisplayAlert("Error",
                    "Unable to open post item page. Please try again.", "OK");
            }
        }

        [RelayCommand]
        private void MyListings()
        {
            Debug.WriteLine("Navigating to MyListings");
            // TODO: Implement My Listings navigation
        }
    }
}