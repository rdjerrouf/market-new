using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Market.DataAccess.Models;
using Market.Market.DataAccess.Models;
using Market.DataAccess.Models.Filters;
using Market.Services;
using System.Diagnostics;
using System.Windows.Input;

namespace Market.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IItemService _itemService;

        #region Properties
        private readonly List<AlState> _states;
        private bool _showFilters;
        public bool ShowFilters
        {
            get => _showFilters;
            set => SetProperty(ref _showFilters, value);
        }

        private ObservableCollection<CategoryOption> _availableCategories = new();
        public ObservableCollection<CategoryOption> AvailableCategories
        {
            get => _availableCategories;
            set => SetProperty(ref _availableCategories, value);
        }

        private ObservableCollection<CategoryOption> _selectedCategories = new();
        public ObservableCollection<CategoryOption> SelectedCategories
        {
            get => _selectedCategories;
            set => SetProperty(ref _selectedCategories, value);
        }
        private readonly ObservableCollection<Item> _items = [];
        public ObservableCollection<Item> Items
        {
            get => _items;
            set
            {
                _items.Clear();
                if (value != null)
                {
                    foreach (var item in value)
                    {
                        _items.Add(item);
                    }
                }
                OnPropertyChanged(nameof(Items));
            }
        }

        private readonly List<SortOption> _sortOptions = [.. Enum.GetValues<SortOption>()];
        public List<SortOption> SortOptions
        {
            get => _sortOptions;
            set
            {
                _sortOptions.Clear();
                if (value != null)
                {
                    _sortOptions.AddRange(value);
                }
                OnPropertyChanged(nameof(SortOptions));
            }
        }
        private decimal _minPrice;
        public decimal MinPrice
        {
            get => _minPrice;
            set
            {
                if (SetProperty(ref _minPrice, value))
                {
                    ApplyFilters().ConfigureAwait(false);
                }
            }
        }

        private decimal _maxPrice = decimal.MaxValue;
        public decimal MaxPrice
        {
            get => _maxPrice;
            set
            {
                if (SetProperty(ref _maxPrice, value))
                {
                    ApplyFilters().ConfigureAwait(false);
                }
            }
        }

        private SortOption _selectedSort = SortOption.Relevance;
        public SortOption SelectedSort
        {
            get => _selectedSort;
            set
            {
                if (SetProperty(ref _selectedSort, value))
                {
                    SortItems();
                }
            }
        }

        private string _searchQuery = string.Empty;
        public string SearchQuery
        {
            get => _searchQuery;
            set => SetProperty(ref _searchQuery, value);
        }

        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private string _title = "Marketplace";
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private AlState? _selectedState;
        public AlState? SelectedState
        {
            get => _selectedState;
            set
            {
                Debug.WriteLine($"SelectedState setter called with value: {value}");
                if (SetProperty(ref _selectedState, value))
                {
                    if (value.HasValue)
                    {
                        MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            Debug.WriteLine($"Calling FilterByState with state: {value}");
                            await FilterByState(value);
                        });
                    }
                    else
                    {
                        MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            Debug.WriteLine("State is null, loading all items");
                            await LoadItemsAsync();
                        });
                    }
                }
            }
        }


        public List<AlState> States => Enum.GetValues<AlState>().OrderBy(s => s.ToString()).ToList(); private ICommand? _searchCommand;
        public ICommand SearchCommand => _searchCommand ??= new Command<string>(async (query) =>
        {
            if (query is not null)
            {
                await SearchItemsAsync(query);
            }
        });

        public enum SortOption
        {
            Relevance,
            PriceLowToHigh,
            PriceHighToLow
        }
        #endregion

        public MainViewModel(IItemService itemService)
        {
            _itemService = itemService ?? throw new ArgumentNullException(nameof(itemService));
            _availableCategories = new ObservableCollection<CategoryOption>();
            _selectedCategories = new ObservableCollection<CategoryOption>();
            _states = new List<AlState>();
            InitializeCategories();
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
        private void InitializeCategories()
        {
            _availableCategories = new ObservableCollection<CategoryOption>
    {
        new() { Name = "For Sale" },
        new() { Name = "Jobs" },
        new() { Name = "Services" },
        new() { Name = "Rentals" }
    };
            OnPropertyChanged(nameof(AvailableCategories));
        }
        [RelayCommand]
        private void ToggleFilters()
        {
            ShowFilters = !ShowFilters;
        }

        [RelayCommand]
        private async Task ClearFilters()
        {
            MinPrice = 0;
            MaxPrice = decimal.MaxValue;
            SelectedSort = SortOption.Relevance;
            SelectedState = null;
            foreach (var category in AvailableCategories)
            {
                category.IsSelected = false;
            }
            await LoadItemsAsync();
        }
        
        #endregion

        #region Sorting Methods
        private void SortItems()
        {
            if (Items.Count == 0) return;

            var sortedItems = SelectedSort switch
            {
                SortOption.PriceLowToHigh => Items.OrderBy(i => i.Price).ToList(),
                SortOption.PriceHighToLow => Items.OrderByDescending(i => i.Price).ToList(),
                SortOption.Relevance => Items.OrderBy(i => i.Title).ToList(),
                _ => Items.ToList()
            };

            Items.Clear();
            foreach (var item in sortedItems)
            {
                Items.Add(item);
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
            Debug.WriteLine($"FilterByState called with state: {state}");
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

        [RelayCommand]
        private async Task ApplyFilters()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                Debug.WriteLine("Applying filters...");

                var allItems = await _itemService.GetItemsAsync();
                var filteredItems = allItems.AsEnumerable();

                // Apply price range filter
                if (MinPrice > 0 || MaxPrice < decimal.MaxValue)
                {
                    Debug.WriteLine($"Applying price filter: {MinPrice} - {MaxPrice}");
                    filteredItems = filteredItems.Where(i => i.Price >= MinPrice && i.Price <= MaxPrice);
                }

                // Apply state filter
                if (SelectedState.HasValue)
                {
                    filteredItems = filteredItems.Where(i => i.State == SelectedState.Value);
                }

                // Apply category filters
                var selectedCategories = AvailableCategories.Where(c => c.IsSelected).Select(c => c.Name).ToList();
                if (selectedCategories.Any())
                {
                    filteredItems = filteredItems.Where(i => selectedCategories.Contains(i.Category));
                }

                // Apply sort
                filteredItems = SelectedSort switch
                {
                    SortOption.PriceLowToHigh => filteredItems.OrderBy(i => i.Price),
                    SortOption.PriceHighToLow => filteredItems.OrderByDescending(i => i.Price),
                    _ => filteredItems.OrderByDescending(i => i.ListedDate)
                };

                Items.Clear();
                foreach (var item in filteredItems)
                {
                    Items.Add(item);
                }

                Debug.WriteLine($"Filter applied. Items count: {Items.Count}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error applying filters: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Unable to apply filters", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}