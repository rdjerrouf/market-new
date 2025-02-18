using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Market.DataAccess.Models;
using Market.Market.DataAccess.Models; // Add this for AlState
using Market.Services;
using System.Diagnostics;
using System.Windows.Input;

namespace Market.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IItemService _itemService;

        #region Properties
        private ObservableCollection<Item> items = new();
        public ObservableCollection<Item> Items
        {
            get => items;
            set => SetProperty(ref items, value);
        }

        private string searchQuery = string.Empty;
        public string SearchQuery
        {
            get => searchQuery;
            set => SetProperty(ref searchQuery, value);
        }

        private bool isRefreshing;
        public bool IsRefreshing
        {
            get => isRefreshing;
            set => SetProperty(ref isRefreshing, value);
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

        private AlState? selectedState;
        public AlState? SelectedState
        {
            get => selectedState;
            set
            {
                if (SetProperty(ref selectedState, value) && value.HasValue)
                {
                    FilterByStateCommand.Execute(value);
                }
            }
        }

        // List of all states for the picker
        public List<AlState> States => Enum.GetValues<AlState>().ToList();

        private ICommand? _searchCommand;
        public ICommand SearchCommand => _searchCommand ??= new Command<string>(async (query) =>
        {
            if (query is not null)
            {
                await SearchItemsAsync(query);
            }
        });
        #endregion

        public MainViewModel(IItemService itemService)
        {
            _itemService = itemService ?? throw new ArgumentNullException(nameof(itemService));
            LoadItemsAsync().ConfigureAwait(false);
        }

        #region Loading Methods
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

        #region Filter Commands
        [RelayCommand]
        private async Task FilterByState(AlState? state)
        {
            if (IsLoading || !state.HasValue) return;

            try
            {
                IsLoading = true;
                Debug.WriteLine($"Filtering items by state: {state}");

                var filteredItems = await _itemService.SearchByStateAsync(state.Value);

                Items.Clear();
                foreach (var item in filteredItems)
                {
                    Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error filtering items by state: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Unable to filter items by state.", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task FilterByCategoryAndState(string category)
        {
            if (IsLoading || !SelectedState.HasValue) return;

            try
            {
                IsLoading = true;
                Debug.WriteLine($"Filtering items by category: {category} and state: {SelectedState}");

                var filteredItems = await _itemService.SearchByCategoryAndStateAsync(category, SelectedState.Value);

                Items.Clear();
                foreach (var item in filteredItems)
                {
                    Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error filtering by category and state: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Unable to filter items.", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }
        #endregion

        #region Category Commands
        [RelayCommand]
        private async Task ForSale()
        {
            if (SelectedState.HasValue)
                await FilterByCategoryAndState("For Sale");
            else
                await FilterByCategoryAsync("For Sale");
        }

        [RelayCommand]
        private async Task Jobs()
        {
            if (SelectedState.HasValue)
                await FilterByCategoryAndState("Jobs");
            else
                await FilterByCategoryAsync("Jobs");
        }

        [RelayCommand]
        private async Task Services()
        {
            if (SelectedState.HasValue)
                await FilterByCategoryAndState("Services");
            else
                await FilterByCategoryAsync("Services");
        }

        [RelayCommand]
        private async Task Rentals()
        {
            if (SelectedState.HasValue)
                await FilterByCategoryAndState("Rentals");
            else
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
        [RelayCommand]
        private static async Task Post()
        {
            await Shell.Current.GoToAsync("AddItemPage");
        }

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

        [RelayCommand]
        private async Task Home()
        {
            SelectedState = null; // Reset state filter
            await LoadItemsAsync();
        }

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

