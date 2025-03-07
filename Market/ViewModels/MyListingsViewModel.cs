using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Market.DataAccess.Models;
using Market.Services;
using System.Diagnostics;

namespace Market.ViewModels
{
    /// <summary>
    /// ViewModel responsible for managing a user's item listings
    /// Implements MVVM pattern with CommunityToolkit.Mvvm
    /// </summary>
    public partial class MyListingsViewModel : ObservableObject
    {
        // Services for item and authentication operations
        private readonly IItemService _itemService;
        private readonly IAuthService _authService;

        // Observable collection to bind items to the UI
        // Will automatically update the UI when items are added or removed

        private ObservableCollection<Item> _items = new();
        public ObservableCollection<Item> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        // Property to track and display loading state

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }
        /// <summary>
        /// Constructor with dependency injection
        /// Initializes services and item collection
        /// </summary>
        /// <param name="itemService">Service for item-related operations</param>
        /// <param name="authService">Service for authentication operations</param>
        public MyListingsViewModel(IItemService itemService, IAuthService authService)
        {
            // Inject required services
            _itemService = itemService;
            _authService = authService;

            // Initialize the items collection
            Items = new ObservableCollection<Item>();
        }

        /// <summary>
        /// Command to load user's listings
        /// Asynchronous method with error handling
        /// </summary>
        [RelayCommand]
        private async Task LoadMyListingsAsync()
        {
            try
            {
                // Set loading state to true to show loading indicator
                IsLoading = true;

                // Attempt to get the current authenticated user
                var currentUser = await _authService.GetCurrentUserAsync();

                // Validate user authentication
                if (currentUser == null)
                {
                    // Prompt user to sign in if not authenticated
                    await Shell.Current.DisplayAlert("Error", "Please sign in to view your listings", "OK");
                    return;
                }

                // Fetch items specific to the current user
                var userItems = await _itemService.GetItemsByUserAsync(currentUser.Id);

                // Clear existing items and add fetched items
                Items.Clear();
                foreach (var item in userItems)
                {
                    Debug.WriteLine($"Item: {item.Title}, PhotoUrl: {item.PhotoUrl ?? "null"}");
                    Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                // Log error for debugging
                Debug.WriteLine($"Error loading my listings: {ex.Message}");

                // Display user-friendly error message
                await Shell.Current.DisplayAlert("Error", "Could not load your listings", "OK");
            }
            finally
            {
                // Ensure loading state is reset
                IsLoading = false;
            }
        }

        /// <summary>
        /// Command to delete a specific item
        /// Includes user confirmation and error handling
        /// </summary>
        /// <param name="item">Item to be deleted</param>
        [RelayCommand]
        private async Task DeleteItemAsync(Item item)
        {
            // Confirm item deletion with user
            bool confirm = await Shell.Current.DisplayAlert(
                "Delete Listing",
                "Are you sure you want to delete this item?",
                "Delete",
                "Cancel");

            // Proceed if user confirms
            if (confirm)
            {
                try
                {
                    // Attempt to delete item through item service
                    bool result = await _itemService.DeleteItemAsync(item.Id);

                    if (result)
                    {
                        // Remove item from local collection if deletion is successful
                        Items.Remove(item);
                    }
                    else
                    {
                        // Display error if item deletion fails
                        await Shell.Current.DisplayAlert("Error", "Could not delete the item", "OK");
                    }
                }
                catch (Exception ex)
                {
                    // Log and display error for failed deletion
                    Debug.WriteLine($"Error deleting item: {ex.Message}");
                    await Shell.Current.DisplayAlert("Error", "An error occurred while deleting the item", "OK");
                }
            }
        }

        [RelayCommand]
        private void OnMyListingsClicked()
        {
            Debug.WriteLine("My Listings button clicked");
            // You can also add a more visible notification:
            Shell.Current.DisplayAlert("Debug", "My Listings button clicked", "OK");
        }
    }
}