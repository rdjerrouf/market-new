using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Market.DataAccess.Models;
using Market.Market.DataAccess.Models;
using Market.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Market.ViewModels
{
    public partial class SearchViewModel : ObservableObject
    {
        private readonly IItemService _itemService;

        #region Properties
        private ObservableCollection<Item> searchResults = [];
        public ObservableCollection<Item> SearchResults
        {
            get => searchResults;
            set => SetProperty(ref searchResults, value);
        }

        private string searchQuery = string.Empty;
        public string SearchQuery
        {
            get => searchQuery;
            set => SetProperty(ref searchQuery, value);
        }

        // Category Filters
        private string? selectedCategory;
        public string? SelectedCategory
        {
            get => selectedCategory;
            set
            {
                if (SetProperty(ref selectedCategory, value))
                    UpdateSubCategories();
            }
        }

        public static List<string> Categories =>[ "For Sale", "Rentals", "Jobs", "Services" ];

        // Subcategories based on main category
        private object? selectedSubCategory;
        public object? SelectedSubCategory
        {
            get => selectedSubCategory;
            set => SetProperty(ref selectedSubCategory, value);
        }

        public List<object> SubCategories { get; private set; } = [];
        

        // State Filter
        private AlState? selectedState;
        public AlState? SelectedState
        {
            get => selectedState;
            set => SetProperty(ref selectedState, value);
        }

        public static List<AlState> States => Enum.GetValues<AlState>().ToList();

        // Price Range
        private decimal minPrice;
        public decimal MinPrice
        {
            get => minPrice;
            set => SetProperty(ref minPrice, value);
        }

        private decimal maxPrice = decimal.MaxValue;
        public decimal MaxPrice
        {
            get => maxPrice;
            set => SetProperty(ref maxPrice, value);
        }

        private bool isBusy;
        public bool IsBusy
        {
            get => isBusy;
            set => SetProperty(ref isBusy, value);
        }
        #endregion

        public SearchViewModel(IItemService itemService)
        {
            _itemService = itemService;
        }

        private void UpdateSubCategories()
        {
            SubCategories.Clear();

            switch (SelectedCategory)
            {
                case "For Sale":
                    SubCategories.AddRange(Enum.GetValues<ForSaleCategory>().Cast<object>());
                    break;
                case "Rentals":
                    SubCategories.AddRange(Enum.GetValues<ForRentCategory>().Cast<object>());
                    break;
                case "Jobs":
                    SubCategories.AddRange(Enum.GetValues<JobCategory>().Cast<object>());
                    break;
                case "Services":
                    SubCategories.AddRange(Enum.GetValues<ServiceCategory>().Cast<object>());
                    break;
            }

            OnPropertyChanged(nameof(SubCategories));
        }

        [RelayCommand]
        private async Task Search()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                Debug.WriteLine("Starting search...");

                var items = await _itemService.GetItemsAsync();
                var filteredItems = items.AsEnumerable();

                // Apply text search
                if (!string.IsNullOrWhiteSpace(SearchQuery))
                {
                    filteredItems = filteredItems.Where(i =>
                        i.Title.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                        i.Description.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));
                }

                // Apply category filter
                if (!string.IsNullOrEmpty(SelectedCategory))
                {
                    filteredItems = filteredItems.Where(i => i.Category == SelectedCategory);
                }

                // Apply subcategory filter
                if (SelectedSubCategory != null)
                {
                    filteredItems = filteredItems.Where(i =>
                    {
                        return SelectedCategory switch
                        {
                            "For Sale" => i.ForSaleCategory == (ForSaleCategory)SelectedSubCategory,
                            "Rentals" => i.ForRentCategory == (ForRentCategory)SelectedSubCategory,
                            "Jobs" => i.JobCategory == (JobCategory)SelectedSubCategory,
                            "Services" => i.ServiceCategory == (ServiceCategory)SelectedSubCategory,
                            _ => true
                        };
                    });
                }

                // Apply state filter
                if (SelectedState.HasValue)
                {
                    filteredItems = filteredItems.Where(i => i.State == SelectedState);
                }

                // Apply price range
                if (MinPrice > 0)
                {
                    filteredItems = filteredItems.Where(i => i.Price >= MinPrice);
                }
                if (MaxPrice < decimal.MaxValue)
                {
                    filteredItems = filteredItems.Where(i => i.Price <= MaxPrice);
                }

                SearchResults.Clear();
                foreach (var item in filteredItems)
                {
                    SearchResults.Add(item);
                }

                Debug.WriteLine($"Found {SearchResults.Count} results");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Search error: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Search failed", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void ClearFilters()
        {
            SearchQuery = string.Empty;
            SelectedCategory = null;
            SelectedSubCategory = null;
            SelectedState = null;
            MinPrice = 0;
            MaxPrice = decimal.MaxValue;
            SearchResults.Clear();
        }
    }
}