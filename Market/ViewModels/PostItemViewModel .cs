using Market.DataAccess.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Market.Services;
using System.Diagnostics;

namespace Market.ViewModels
{
    /// <summary>
    /// ViewModel for handling item posting functionality.
    /// Uses ObservableObject for property change notifications.
    /// </summary>
    public partial class PostItemViewModel : ObservableObject
    {
        // Services for data operations
        private readonly IItemService _itemService;
        private readonly IAuthService _authService;

        // Observable properties for form fields
        [ObservableProperty]
        private string _title = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private decimal _price;

        [ObservableProperty]
        private string _category = "For Sale";  // Default category

        [ObservableProperty]
        private string? _photoUrl;

        [ObservableProperty]
        private bool _isBusy;

        /// <summary>
        /// Constructor - initializes services needed for item operations
        /// </summary>
        public PostItemViewModel(IItemService itemService, IAuthService authService)
        {
            _itemService = itemService;
            _authService = authService;
            Debug.WriteLine("PostItemViewModel initialized");
        }

        /// <summary>
        /// Validates if the item can be saved based on required fields
        /// </summary>
        private bool CanSaveItem()
        {
            return !IsBusy &&
                   !string.IsNullOrWhiteSpace(Title) &&
                   !string.IsNullOrWhiteSpace(Description) &&
                   Price > 0;
        }

        /// <summary>
        /// Handles saving the new item to the database
        /// Uses RelayCommand with CanExecute for validation
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanSaveItem))]
        private async Task SaveItemAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                Debug.WriteLine("Starting item save process...");

                // Create new item from form data
                var item = new Item
                {
                    Title = Title,
                    Description = Description,
                    Price = Price,
                    PhotoUrl = PhotoUrl,
                    Status = Category,
                    ListedDate = DateTime.UtcNow,
                    UserId = 1 // TODO: Get actual user ID from AuthService
                };

                // Save to database
                var success = await _itemService.AddItemAsync(item);

                if (success)
                {
                    Debug.WriteLine("Item saved successfully");
                    await Shell.Current.DisplayAlert("Success", "Item posted successfully!", "OK");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    Debug.WriteLine("Failed to save item");
                    await Shell.Current.DisplayAlert("Error", "Failed to post item. Please try again.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving item: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Handles photo selection and temporary storage
        /// In a real app, would upload to a server
        /// </summary>
        [RelayCommand]
        private async Task UploadPhotoAsync()
        {
            if (IsBusy) return;

            try
            {
                Debug.WriteLine("Starting photo upload process...");

                // Open photo picker
                var photo = await MediaPicker.PickPhotoAsync();
                if (photo != null)
                {
                    // Store photo path (in real app, would upload to server)
                    PhotoUrl = photo.FullPath;
                    Debug.WriteLine($"Photo selected: {PhotoUrl}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Photo upload error: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", $"Failed to upload photo: {ex.Message}", "OK");
            }
        }
    }
}