using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Market.DataAccess.Models;
using Market.Services;
using System.Diagnostics;
using System.Windows.Input;

namespace Market.ViewModels
{
    /// <summary>
    /// ViewModel for the main marketplace page
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        private readonly IItemService _itemService;
        
        /// <summary>
        /// Collection of items displayed in the marketplace
        /// </summary>
        public ObservableCollection<Item> Items { get; } = new();

        // Replace fields with full property implementations
        private string searchQuery = string.Empty;
        public string SearchQuery
        {
            get => searchQuery;
            set => SetProperty(ref searchQuery, value);
        }

        private bool isLoading;
        public bool IsLoading
        {
            get => isLoading;
            set => SetProperty(ref isLoading, value);
        }

        private string title = "Marketplace";
        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        private ICommand? _searchCommand;
        public ICommand SearchCommand => _searchCommand ??= new Command<string>(async (query) =>
        {
            if (query is not null)
            {
                await SearchItemsAsync(query);
            }
        });

        public MainViewModel(IItemService itemService)
        {
            _itemService = itemService ?? throw new ArgumentNullException(nameof(itemService));
            LoadItemsAsync().ConfigureAwait(false);
        }

        // Rest of your existing methods remain exactly the same...
        private async Task LoadItemsAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                Debug.WriteLine("Loading items...");

                var allItems = await _itemService.GetItemsAsync();
                Items.Clear();
                foreach (var item in allItems)
                {
                    Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading items: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Unable to load items.", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SearchItemsAsync(string query)
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;

                var allItems = await _itemService.GetItemsAsync();
                var filteredItems = string.IsNullOrWhiteSpace(query)
                    ? allItems
                    : allItems.Where(item =>
                        item.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        item.Description.Contains(query, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                Items.Clear();
                foreach (var item in filteredItems)
                {
                    Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during search: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Search failed.", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task ForSale()
        {
            await FilterByCategoryAsync("For Sale");
        }

        [RelayCommand]
        private async Task Jobs()
        {
            await FilterByCategoryAsync("Jobs");
        }

        [RelayCommand]
        private async Task Services()
        {
            await FilterByCategoryAsync("Services");
        }

        [RelayCommand]
        private async Task Rentals()
        {
            await FilterByCategoryAsync("Rentals");
        }

        private async Task FilterByCategoryAsync(string category)
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;

                var allItems = await _itemService.GetItemsAsync();
                var filteredItems = allItems
                    .Where(item => item.Status.Equals(category, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                Items.Clear();
                foreach (var item in filteredItems)
                {
                    Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error filtering {category} items: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Unable to filter items.", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task Post()
        {
            try
            {
                await Shell.Current.GoToAsync("PostItemPage");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation error: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Unable to open post page.", "OK");
            }
        }

        [RelayCommand]
        private async Task Inbox()
        {
            Debug.WriteLine("Inbox function called");

            try
            {
                Debug.WriteLine("Attempting to navigate to InboxPage");
                // Navigate to inbox page
                await Shell.Current.GoToAsync("InboxPage");
                Debug.WriteLine("Navigation to InboxPage completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation error to Inbox: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", $"Unable to open inbox: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task MyListings()
        {
            Debug.WriteLine("MyListings function has started");
            try
            {
                // To implement this, we'll need to add a dependency for IAuthService
                // Assuming you have an authentication service that can provide the current user's ID
                var currentUserId = 1; // TODO: Replace with actual user ID retrieval

                Items.Clear();

                // Fetch items specific to the current user
                var userItems = await _itemService.GetItemsByUserAsync(currentUserId);

                Debug.WriteLine($"MyListings: Retrieved {userItems.Count()} items from service");

                // Populate items collection
                foreach (var item in userItems)
                {
                    Items.Add(item);
                    Debug.WriteLine($"MyListings: Added item: ID={item.Id}, Title={item.Title}");
                }

                // Update title to reflect current view
                Title = "My Listings";
               
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching my listings: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Unable to load your listings", "OK");
            }
        }

                    
        [RelayCommand]
        private async Task Home()
        {
            await LoadItemsAsync();
        }
    }
}