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
    /// ViewModel for the main marketplace page. Handles item display, filtering,
    /// and user interactions for the main marketplace interface.
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        private readonly IItemService _itemService;

        #region Properties
        /// <summary>
        /// Collection of marketplace items displayed to the user
        /// </summary>
        private ObservableCollection<Item> items = new();
        public ObservableCollection<Item> Items
        {
            get => items;
            set => SetProperty(ref items, value);
        }

        /// <summary>
        /// Current search query entered by the user
        /// </summary>
        private string searchQuery = string.Empty;
        public string SearchQuery
        {
            get => searchQuery;
            set => SetProperty(ref searchQuery, value);
        }

        /// <summary>
        /// Indicates whether the view is currently refreshing
        /// </summary>
        private bool isRefreshing;
        public bool IsRefreshing
        {
            get => isRefreshing;
            set => SetProperty(ref isRefreshing, value);
        }

        /// <summary>
        /// Indicates whether data loading operations are in progress
        /// </summary>
        private bool isLoading;
        public bool IsLoading
        {
            get => isLoading;
            set => SetProperty(ref isLoading, value);
        }

        /// <summary>
        /// Title displayed at the top of the marketplace page
        /// </summary>
        private string title = "Marketplace";
        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        /// <summary>
        /// Command to handle search operations
        /// </summary>
        private ICommand? _searchCommand;
        public ICommand SearchCommand => _searchCommand ??= new Command<string>(async (query) =>
        {
            if (query is not null)
            {
                await SearchItemsAsync(query);
            }
        });
        #endregion

        /// <summary>
        /// Initializes a new instance of the MainViewModel
        /// </summary>
        /// <param name="itemService">Service for managing marketplace items</param>
        /// <exception cref="ArgumentNullException">Thrown when itemService is null</exception>
        public MainViewModel(IItemService itemService)
        {
            _itemService = itemService ?? throw new ArgumentNullException(nameof(itemService));
            LoadItemsAsync().ConfigureAwait(false);
        }

        #region Loading Methods
        /// <summary>
        /// Loads or reloads all marketplace items from the service
        /// </summary>
        private async Task LoadItemsAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                Debug.WriteLine("Loading items...");

                var allItems = await _itemService.GetItemsAsync();
                
                Debug.WriteLine($"Loaded {allItems.Count()} items from service");

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
                Debug.WriteLine("Finished loading items");

            }
        }
        #endregion

        #region Search Methods
        /// <summary>
        /// Filters items based on user search query
        /// </summary>
        /// <param name="query">Search terms entered by user</param>
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
        #endregion

        #region Category Commands
        /// <summary>
        /// Filters items to show only items for sale
        /// </summary>
        [RelayCommand]
        private async Task ForSale()
        {
            await FilterByCategoryAsync("For Sale");
        }

        /// <summary>
        /// Filters items to show only job listings
        /// </summary>
        [RelayCommand]
        private async Task Jobs()
        {
            await FilterByCategoryAsync("Jobs");
        }

        /// <summary>
        /// Filters items to show only service offerings
        /// </summary>
        [RelayCommand]
        private async Task Services()
        {
            await FilterByCategoryAsync("Services");
        }

        /// <summary>
        /// Filters items to show only rental listings
        /// </summary>
        [RelayCommand]
        private async Task Rentals()
        {
            await FilterByCategoryAsync("Rentals");
        }

        /// <summary>
        /// Filters the item list by specified category
        /// </summary>
        /// <param name="category">Category to filter by</param>
        private async Task FilterByCategoryAsync(string category)
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;

                var allItems = await _itemService.GetItemsAsync();
                var filteredItems = allItems
                    .Where(item => item.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
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
        #endregion

        #region Navigation Commands
        /// <summary>
        /// Navigates to the add item page
        /// </summary>
        [RelayCommand]
        private static async Task Post()
        {
            await Shell.Current.GoToAsync("AddItemPage");
        }

        /// <summary>
        /// Navigates to the user's inbox
        /// </summary>
        [RelayCommand]
        private static async Task Inbox()
        {
            Debug.WriteLine("Inbox function called");

            try
            {
                Debug.WriteLine("Attempting to navigate to InboxPage");
                await Shell.Current.GoToAsync("InboxPage");
                Debug.WriteLine("Navigation to InboxPage completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation error to Inbox: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", $"Unable to open inbox: {ex.Message}", "OK");
            }
        }

        /// <summary>
        /// Loads and displays the current user's listings
        /// </summary>
        [RelayCommand]
        private async Task MyListings()
        {
            Debug.WriteLine("MyListings function has started");
            try
            {
                var currentUserId = 1; // TODO: Replace with actual user ID retrieval
                Items.Clear();

                var userItems = await _itemService.GetItemsByUserAsync(currentUserId);
                Debug.WriteLine($"MyListings: Retrieved {userItems.Count()} items from service");

                foreach (var item in userItems)
                {
                    Items.Add(item);
                    Debug.WriteLine($"MyListings: Added item: ID={item.Id}, Title={item.Title}");
                }

                Title = "My Listings";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching my listings: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Unable to load your listings", "OK");
            }
        }

        /// <summary>
        /// Returns to the home view and reloads all items
        /// </summary>
        [RelayCommand]
        private async Task Home()
        {
            await LoadItemsAsync();
        }

        /// <summary>
        /// Navigates to the account management page
        /// </summary>
        [RelayCommand]
        private static async Task Account()
        {
            try
            {
                Debug.WriteLine("Account command executed");
                await Shell.Current.GoToAsync("///SignInPage");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation error in AccountCommand: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Unable to navigate to account page", "OK");
            }
        }
        #endregion

        #region Refresh Command
        /// <summary>
        /// Refreshes the items list when triggered by pull-to-refresh
        /// </summary>
        [RelayCommand]
        private async Task Refresh()
        {
            try
            {
                IsRefreshing = true;
                await LoadItemsAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in RefreshCommand: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Unable to refresh items", "OK");
            }
            finally
            {
                IsRefreshing = false;
            }
        }
        #endregion
    }
}