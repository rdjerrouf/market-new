using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Market.Services;
using Market.DataAccess.Models;
using System.Diagnostics;

namespace Market.ViewModels.AddItem
{
    /// <summary>
    /// ViewModel for the For Sale item creation page
    /// Handles form validation and item submission for items being sold
    /// </summary>
    public partial class ForSaleItemViewModel : ObservableObject
    {
        private readonly IItemService _itemService;
        private readonly IAuthService _authService;

        // Validation constants
        private const int TITLE_MIN_LENGTH = 3;
        private const int TITLE_MAX_LENGTH = 100;
        private const int DESCRIPTION_MIN_LENGTH = 5;
        private const decimal MIN_PRICE = 0.01m;
        private const decimal MAX_PRICE = 999999.99m;
        private const long MAX_PHOTO_SIZE = 5 * 1024 * 1024; // 5MB

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public ForSaleItemViewModel(IItemService itemService, IAuthService authService)
        {
            _itemService = itemService;
            _authService = authService;
            title = string.Empty; // Initialize title
            description = string.Empty; // Initialize description
            Debug.WriteLine("ForSaleItemViewModel initialized");
        }
        #region Properties

        private string title;
        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }


        private string description;
        public string Description
        {
            get => description;
            set => SetProperty(ref description, value);
        }

        private decimal price;
        public decimal Price
        {
            get => price;
            set => SetProperty(ref price, value);
        }

        private string? photoUrl;
        public string? PhotoUrl
        {
            get => photoUrl;
            set => SetProperty(ref photoUrl, value);
        }

        private bool isBusy;
        public bool IsBusy
        {
            get => isBusy;
            set => SetProperty(ref isBusy, value);
        }

        // Add this to the computed properties section
        public bool CanSave => !IsBusy &&
                               !HasTitleError &&
                               !HasDescriptionError &&
                               !HasPriceError;
        // Error message properties
        private string? titleError;
        public string? TitleError
        {
            get => titleError;
            set => SetProperty(ref titleError, value);
        }

        private string? descriptionError;
        public string? DescriptionError
        {
            get => descriptionError;
            set => SetProperty(ref descriptionError, value);
        }

        private string? priceError;
        public string? PriceError
        {
            get => priceError;
            set => SetProperty(ref priceError, value);
        }

        // Computed properties
        public bool IsNotBusy => !IsBusy;
        public bool HasPhoto => !string.IsNullOrEmpty(PhotoUrl);
        public bool HasTitleError => !string.IsNullOrEmpty(TitleError);
        public bool HasDescriptionError => !string.IsNullOrEmpty(DescriptionError);
        public bool HasPriceError => !string.IsNullOrEmpty(PriceError);

        #endregion

        #region Validation Methods

        /// <summary>
        /// Validates the item title
        /// </summary>
        private bool ValidateTitle()
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                TitleError = "Please enter a title";
                return false;
            }

            if (Title.Length < TITLE_MIN_LENGTH)
            {
                TitleError = $"Title must be at least {TITLE_MIN_LENGTH} characters";
                return false;
            }

            TitleError = null;
            Debug.WriteLine("Title validated");
            return true;
        }

        /// <summary>
        /// Validates the item description
        /// </summary>
        private bool ValidateDescription()
        {
            if (string.IsNullOrWhiteSpace(Description))
            {
                DescriptionError = "Please describe your item";
                return false;
            }

            if (Description.Length < DESCRIPTION_MIN_LENGTH)
            {
                DescriptionError = $"Description must be at least {DESCRIPTION_MIN_LENGTH} characters";
                return false;
            }

            DescriptionError = null;
            Debug.WriteLine("Description is VALIDATED");
            return true;
        }

        /// <summary>
        /// Validates the item price
        /// </summary>
        private bool ValidatePrice()
        {
            if (Price < MIN_PRICE)
            {
                PriceError = "Price must be greater than zero";
                return false;
            }

            if (Price > MAX_PRICE)
            {
                PriceError = $"Price cannot exceed {MAX_PRICE:C}";
                return false;
            }

            PriceError = null;
            
            Debug.WriteLine("Price validated");
            return true;
        }

        #endregion

        #region Commands

        /// <summary>
        /// Handles photo upload functionality
        /// </summary>
        [RelayCommand]
        private async Task UploadPhoto()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                Debug.WriteLine("Starting photo upload process");

                var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Select a photo"
                });

                if (result == null)
                {
                    Debug.WriteLine("No photo selected");
                    return;
                }

                Debug.WriteLine($"Photo selected: {result.FileName}");

                if (!await ValidatePhoto(result))
                {
                    Debug.WriteLine("Photo validation failed");
                    return;
                }

                var photoPath = await SavePhoto(result);
                if (photoPath != null)
                {
                    PhotoUrl = photoPath;
                    OnPropertyChanged(nameof(HasPhoto));
                    Debug.WriteLine($"Photo saved successfully at: {photoPath}");
                    await Shell.Current.DisplayAlert("Success", "Photo uploaded successfully", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Photo upload error: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                await Shell.Current.DisplayAlert("Error",
                    "Failed to upload photo. Please try again.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Saves the For Sale item
        /// </summary>

        [RelayCommand]
        private async Task Save()
        {
            if (IsBusy) return;
            try
            {
                IsBusy = true;
                Debug.WriteLine("Starting save process...");
                // Validation check
                if (!ValidateAll())
                {
                    Debug.WriteLine("Validation failed");
                    return;
                }
                Debug.WriteLine("Validation passed");
                // Get current user ID
                Debug.WriteLine("Getting current user ID...");
                int userId;
                try
                {
                    userId = await _authService.GetCurrentUserIdAsync();
                    Debug.WriteLine($"Got user ID: {userId}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to get current user ID: {ex.Message}");
                    Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                    await Shell.Current.DisplayAlert("Error", "Please sign in to post items", "OK");
                    await Shell.Current.GoToAsync("///SignInPage");
                    return;
                }
                // Create item
                Debug.WriteLine("Creating item object...");
                var item = new Item
                {
                    Title = Title,
                    Description = Description,
                    Price = Price,
                    Category = "For Sale",
                    PhotoUrl = PhotoUrl,
                    ListedDate = DateTime.UtcNow,
                    UserId = userId
                };
                Debug.WriteLine($"Item created: Title={item.Title}, Price={item.Price}, PhotoUrl={item.PhotoUrl}");
                // Save item
                Debug.WriteLine("Attempting to save item to database...");
                var result = await _itemService.AddItemAsync(item);
                if (result)
                {
                    Debug.WriteLine("Item saved successfully");
                    await Shell.Current.DisplayAlert("Success", "Your item has been posted!", "OK");
                    await Shell.Current.GoToAsync("//MainPage");
                }
                else
                {
                    Debug.WriteLine("Failed to save item to database");
                    await Shell.Current.DisplayAlert("Error", "Failed to post item", "OK");
                }
                IsBusy = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Save error: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    Debug.WriteLine($"Inner stack trace: {ex.InnerException.StackTrace}");
                }
                await Shell.Current.DisplayAlert("Error", "An error occurred while saving", "OK");
            }
            finally
            {
                Debug.WriteLine("Save process completed");
            }
        }
        #endregion

        #region Helper Methods

        /// <summary>
        /// Validates all form fields
        /// </summary>
        private bool ValidateAll()
        {
            return ValidateTitle()
                   && ValidateDescription()
                   && ValidatePrice();
        }

        /// <summary>
        /// Validates the uploaded photo
        /// </summary>
        private static async Task<bool> ValidatePhoto(FileResult photo)
        {
            try
            {
                using var stream = await photo.OpenReadAsync();
                if (stream.Length > MAX_PHOTO_SIZE)
                {
                    Debug.WriteLine($"Photo size ({stream.Length / 1024 / 1024}MB) exceeds limit of {MAX_PHOTO_SIZE / 1024 / 1024}MB");
                    await Shell.Current.DisplayAlert("Error", "Photo must be less than 5MB", "OK");
                    return false;
                }

                var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();
                if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
                {
                    Debug.WriteLine($"Invalid file extension: {extension}");
                    await Shell.Current.DisplayAlert("Error", "Please select a JPG or PNG file", "OK");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Photo validation error: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to validate photo", "OK");
                return false;
            }
        }

        /// <summary>
        /// Saves the photo to local storage
        /// </summary>
        private static async Task<string?> SavePhoto(FileResult photo)
        {
            try
            {
                // Use MAUI's cache directory for photos
                var photosDir = Path.Combine(FileSystem.CacheDirectory, "Photos");
                if (!Directory.Exists(photosDir))
                    Directory.CreateDirectory(photosDir);

                // Generate a unique filename
                var fileName = $"item_photo_{Guid.NewGuid()}{Path.GetExtension(photo.FileName)}";
                var localPath = Path.Combine(photosDir, fileName);

                Debug.WriteLine($"Saving photo to: {localPath}");

                using (var sourceStream = await photo.OpenReadAsync())
                using (var destinationStream = File.Create(localPath))
                {
                    await sourceStream.CopyToAsync(destinationStream);
                }

                Debug.WriteLine("Photo saved successfully");
                return localPath;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Photo save error: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw; // Rethrow to handle in the calling method
            }
        }

        #endregion
    }
}