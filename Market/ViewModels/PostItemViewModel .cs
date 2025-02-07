// Import necessary namespaces for functionality
using Market.DataAccess.Models;
using Market.Converters;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Market.Services;
using System.Diagnostics;

namespace Market.ViewModels
{
    /// <summary>
    /// ViewModel responsible for handling the creation and posting of new marketplace items.
    /// Includes photo upload functionality, form validation, and data persistence.
    /// </summary>
    public partial class PostItemViewModel : ObservableObject
    {
        // Dependencies injected through constructor
        private readonly IItemService _itemService;
        private readonly IAuthService _authService;

        // Constants for form validation
        // These define the constraints for item properties
        private const int TITLE_MIN_LENGTH = 3;
        private const int TITLE_MAX_LENGTH = 100;
        private const int DESCRIPTION_MIN_LENGTH = 8;
        private const int DESCRIPTION_MAX_LENGTH = 1000;
        private const decimal MIN_PRICE = 0.01m;
        private const decimal MAX_PRICE = 999999.99m;
        private const long MAX_PHOTO_SIZE = 5 * 1024 * 1024; // 5MB in bytes

        #region Error Message Properties
        // Properties to hold validation error messages
        // These are displayed in the UI when validation fails

        /// <summary>
        /// Error message for title validation
        /// </summary>
        private string? _titleError;
        public string? TitleError
        {
            get => _titleError;
            set => SetProperty(ref _titleError, value);
        }

        /// <summary>
        /// Error message for description validation
        /// </summary>
        private string? _descriptionError;
        public string? DescriptionError
        {
            get => _descriptionError;
            set => SetProperty(ref _descriptionError, value);
        }

        /// <summary>
        /// Error message for price validation
        /// </summary>
        private string? _priceError;
        public string? PriceError
        {
            get => _priceError;
            set => SetProperty(ref _priceError, value);
        }

        /// <summary>
        /// Error message for category validation
        /// </summary>
        private string? _categoryError;
        public string? CategoryError
        {
            get => _categoryError;
            set => SetProperty(ref _categoryError, value);
        }
        #endregion

        #region Form Field Properties
        // Properties for the item form fields
        // These bind to the UI and hold the user input

        /// <summary>
        /// Title of the item being posted
        /// Triggers validation on change
        /// </summary>
        private string title = string.Empty;
        public string Title
        {
            get => title;
            set
            {
                SetProperty(ref title, value);
                ValidateTitle();
                SaveItemCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Description of the item being posted
        /// Triggers validation on change
        /// </summary>
        private string description = string.Empty;
        public string Description
        {
            get => description;
            set
            {
                SetProperty(ref description, value);
                ValidateDescription();
                SaveItemCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Price of the item being posted
        /// Triggers validation on change
        /// </summary>
        private decimal price = 1.00M;
        public decimal Price
        {
            get => price;
            set
            {
                SetProperty(ref price, value);
                ValidatePrice();
                SaveItemCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Category of the item being posted
        /// Triggers validation on change
        /// Default value is "For Sale"
        /// </summary>
        private string _category = "For Sale";
        public string Category
        {
            get => _category;
            set
            {
                SetProperty(ref _category, value);
                ValidateCategory();
                SaveItemCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Path to the uploaded photo
        /// This is the local file system path where the photo is stored
        /// </summary>
        private string? _photoUrl;
        public string? PhotoUrl
        {
            get => _photoUrl;
            set => SetProperty(ref _photoUrl, value);
        }

        /// <summary>
        /// Indicates if the ViewModel is currently processing an operation
        /// Used to prevent multiple simultaneous operations
        /// </summary>
        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                SetProperty(ref _isBusy, value);
                SaveItemCommand.NotifyCanExecuteChanged();
            }
        }
        #endregion

        /// <summary>
        /// Constructor for PostItemViewModel
        /// Initializes services and sets up initial state
        /// </summary>
        /// <param name="itemService">Service for managing marketplace items</param>
        /// <param name="authService">Service for user authentication</param>
        public PostItemViewModel(IItemService itemService, IAuthService authService)
        {
            _itemService = itemService ?? throw new ArgumentNullException(nameof(itemService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            Debug.WriteLine("PostItemViewModel initialized");
        }

        #region Validation Methods
        // Methods to validate form fields
        // Each returns true if validation passes, false otherwise

        /// <summary>
        /// Validates the item title
        /// Checks for required field, minimum and maximum length
        /// </summary>
        private bool ValidateTitle()
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                TitleError = "Title is required";
                return false;
            }

            if (Title.Length < TITLE_MIN_LENGTH)
            {
                TitleError = $"Title must be at least {TITLE_MIN_LENGTH} characters";
                return false;
            }

            if (Title.Length > TITLE_MAX_LENGTH)
            {
                TitleError = $"Title cannot exceed {TITLE_MAX_LENGTH} characters";
                return false;
            }

            TitleError = null;
            return true;
        }

        /// <summary>
        /// Validates the item description
        /// Checks for required field, minimum and maximum length
        /// </summary>
        private bool ValidateDescription()
        {
            if (string.IsNullOrWhiteSpace(Description))
            {
                DescriptionError = "Description is required";
                return false;
            }

            if (Description.Length < DESCRIPTION_MIN_LENGTH)
            {
                DescriptionError = $"Description must be at least {DESCRIPTION_MIN_LENGTH} characters";
                return false;
            }

            if (Description.Length > DESCRIPTION_MAX_LENGTH)
            {
                DescriptionError = $"Description cannot exceed {DESCRIPTION_MAX_LENGTH} characters";
                return false;
            }

            DescriptionError = null;
            return true;
        }

        /// <summary>
        /// Validates the item price
        /// Checks for minimum and maximum price limits
        /// </summary>
        private bool ValidatePrice()
        {
            Console.WriteLine($"Debug - Validating Price: {Price}"); // Add current price value

            if (Price < MIN_PRICE)
            {
                Console.WriteLine($"Debug - Price {Price} is less than MIN_PRICE {MIN_PRICE}"); // Add comparison debug
                PriceError = "Price must be greater than zero";
                return false;
            }
            if (Price > MAX_PRICE)
            {
                PriceError = $"Price cannot exceed {MAX_PRICE:C}";
                return false;
            }
            PriceError = null;
            return true;
        }

        private string _selectedCategory = "For Sale";
        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                SetProperty(ref _selectedCategory, value);
                Category = value; // Update the Category property 
            }
        }
        /// <summary>
        /// Type of job for Jobs category
        /// </summary>
       
        private string? _jobType;
        public string? JobType
        {
            get => _jobType;
            set => SetProperty(ref _jobType, value);
        }
        /// <summary>
        /// Type of service for Services category
        /// </summary>

        private string? serviceType;
        public string? ServiceType
        {
            get => serviceType;
            set => SetProperty(ref serviceType, value);
        }

        /// <summary>
        /// Rental period for Rentals category
        /// </summary>
        
        private string? rentalPeriod;
        public string? RentalPeriod
        {
            get => rentalPeriod;
            set => SetProperty(ref rentalPeriod, value);
        }

        /// <summary>
        /// Start date for job or rental availability
        /// </summary>
        
        private DateTime? availableFrom;
        public DateTime? AvailableFrom
        {
            get => availableFrom;
            set => SetProperty(ref availableFrom, value);
        }

        /// <summary>
        /// End date for rental availability
        /// </summary>
        
        private DateTime? availableTo;
        public DateTime? AvailableTo
        {
            get => availableTo;
            set => SetProperty(ref availableTo, value);
        }

        // ... existing validation methods ...

        /// <summary>
        /// Validates job-specific details when Jobs category is selected
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        private bool ValidateJobDetails()
        {
            if (SelectedCategory == "Jobs" && string.IsNullOrEmpty(JobType))
            {
                CategoryError = "Job type is required for Jobs category";
                return false;
            }
            return true;
        }

        /// <summary>
        /// Validates service-specific details when Services category is selected
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        private bool ValidateServiceDetails()
        {
            if (SelectedCategory == "Services" && string.IsNullOrEmpty(ServiceType))
            {
                CategoryError = "Service type is required for Services category";
                return false;
            }
            return true;
        }

        /// <summary>
        /// Validates rental-specific details when Rentals category is selected
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        private bool ValidateRentalDetails()
        {
            if (SelectedCategory == "Rentals")
            {
                if (string.IsNullOrEmpty(RentalPeriod))
                {
                    CategoryError = "Rental period is required for Rentals category";
                    return false;
                }
                if (!AvailableFrom.HasValue || !AvailableTo.HasValue)
                {
                    CategoryError = "Available dates are required for Rentals category";
                    return false;
                }
                if (AvailableFrom > AvailableTo)
                {
                    CategoryError = "Available from date must be before available to date";
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// Validates the item category
        /// Checks if the category is one of the allowed values
        /// </summary>
        private bool ValidateCategory()
        {
            var validCategories = new[] { "For Sale", "Jobs", "Services", "Rentals" };

            if (string.IsNullOrEmpty(Category))
            {
                CategoryError = "Category is required";
                return false;
            }

            if (!validCategories.Contains(Category))
            {
                CategoryError = "Invalid category selected";
                return false;
            }

            CategoryError = null;
            return true;
        }

        // ... (rest of the code)

        /// <summary>
        /// Determines if the item can be saved
        /// Checks all validation rules and busy state
        /// </summary>
        private bool CanSaveItem()
        {
            var isValid = !IsBusy &&
                         ValidateTitle() &&
                         ValidateDescription() &&
                         ValidatePrice() &&
                         ValidateCategory();

            // Perform category-specific validations
            switch (SelectedCategory)
            {
                case "Jobs":
                    isValid &= ValidateJobDetails();
                    break;
                case "Services":
                    isValid &= ValidateServiceDetails();
                    break;
                case "Rentals":
                    isValid &= ValidateRentalDetails();
                    break;
            }

            Debug.WriteLine($"CanSaveItem check - " +
                           $"IsBusy: {IsBusy}, " +
                           $"Title valid: {TitleError == null}, " +
                           $"Description valid: {DescriptionError == null}, " +
                           $"Price valid: {PriceError == null}, " +
                           $"Category valid: {CategoryError == null}, " +
                           $"Final result: {isValid}");

            return isValid;
        }
        #endregion

        #region Photo Handling Methods
        /// <summary>
        /// Handles the photo upload process
        /// Includes picking a photo, validation, and saving to local storage
        /// </summary>
        [RelayCommand]
        private async Task UploadPhotoAsync()
        {
            Debug.WriteLine("UploadPhotoAsync called");

            if (IsBusy) return;

            string? tempPhotoPath = null;
            try
            {
                IsBusy = true;
                Debug.WriteLine("Starting photo upload");

                // Check permissions on iOS/MacCatalyst
#if IOS || MACCATALYST
                if (!await CheckPhotoPermissions()) return;
#endif

                // Let user pick a photo
                var photo = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Select a photo"

                });
                Debug.WriteLine($"Photo picked: before checking if phote is  null");
                if (photo == null) return;

                Debug.WriteLine($"Photo selected: {photo.FileName}");

                // Validate the selected photo
                if (!await ValidatePhotoAsync(photo)) return;
                Debug.WriteLine("Photo validation passed");
                // Create storage directory if it doesn't exist
                var localStorageDir = Path.Combine(FileSystem.AppDataDirectory, "ItemPhotos");
                Directory.CreateDirectory(localStorageDir);

                Debug.WriteLine($"Local storage directory: {localStorageDir}");

                // Generate unique filename for the photo
                var fileName = $"item_photo_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(photo.FileName)}";
                tempPhotoPath = Path.Combine(localStorageDir, fileName);

                Debug.WriteLine($"Temp photo path: {tempPhotoPath}");

                // Copy photo to app storage
                using (var sourceStream = await photo.OpenReadAsync())
                using (var destinationStream = File.OpenWrite(tempPhotoPath))
                {
                    await sourceStream.CopyToAsync(destinationStream);

                    Debug.WriteLine($"Photo copied to: {tempPhotoPath}");
                }

                PhotoUrl = tempPhotoPath;
                Debug.WriteLine($"Photo saved: {PhotoUrl}");
                await Shell.Current.DisplayAlert("Success", "Photo uploaded", "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Upload error: {ex.Message}");
                if (tempPhotoPath != null) CleanupPhoto(tempPhotoPath);
                await Shell.Current.DisplayAlert("Error", $"Upload failed: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Cleans up temporary photo files
        /// Called when upload fails or item save fails
        /// </summary>
        private void CleanupPhoto(string? photoPath)
        {
            if (string.IsNullOrEmpty(photoPath)) return;

            try
            {
                if (File.Exists(photoPath))
                {
                    File.Delete(photoPath);
                    Debug.WriteLine($"Cleaned up photo: {photoPath}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error cleaning up photo: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates the selected photo
        /// Checks file size and format
        /// </summary>
        private static async Task<bool> ValidatePhotoAsync(FileResult photo)
        {
            Debug.WriteLine($"Validating photo: {photo.FileName}");

            // Check file size (max 5MB)
            using var stream = await photo.OpenReadAsync();
            if (stream.Length > MAX_PHOTO_SIZE)
            {
                await Shell.Current.DisplayAlert("Error", "Photo must be less than 5MB", "OK");
                return false;
            }

            // Check file extension
            var validExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();
            if (!validExtensions.Contains(extension))
            {
                await Shell.Current.DisplayAlert("Error", "Only .jpg and .png files are allowed", "OK");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks and requests necessary permissions for photo access
        /// Handles iOS/MacCatalyst specific permission requirements
        /// </summary>
        private async Task<bool> CheckPhotoPermissions()
        {
            // Check storage permissions
            var storageStatus = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            if (storageStatus != PermissionStatus.Granted)
            {
                storageStatus = await Permissions.RequestAsync<Permissions.StorageRead>();
                if (storageStatus != PermissionStatus.Granted)
                {
                    await Shell.Current.DisplayAlert("Permission Required",
                        "Storage permission needed for photos", "OK");
                    return false;
                }
            }

            // Check photo library permissions on iOS/MacCatalyst
            if (OperatingSystem.IsIOSVersionAtLeast(14, 0) ||
                OperatingSystem.IsMacCatalystVersionAtLeast(14, 0))
            {
                var photosStatus = await Permissions.CheckStatusAsync<Permissions.Photos>();
                if (photosStatus != PermissionStatus.Granted)
                {
                    photosStatus = await Permissions.RequestAsync<Permissions.Photos>();
                    if (photosStatus != PermissionStatus.Granted)
                    {
                        await Shell.Current.DisplayAlert("Permission Required",
                            "Photos permission needed", "OK");
                        return false;
                    }
                }
            }
            return true;
        }
        #endregion

        #region Save Item Command
        /// <summary>
        /// Saves the item to the marketplace
        /// Validates all fields and handles photo path storage
        /// </summary>

        /// <summary>
        /// Saves the item to the database
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanSaveItem))]
#pragma warning disable CS1998 // Async method lacks 'await' operators
        private async Task SaveItem()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                Debug.WriteLine("Starting item save");

                // Create new item with all relevant fields
                var item = new Item
                {
                    Title = Title,
                    Description = Description,
                    Price = Price,
                    Category = SelectedCategory,
                    PhotoUrl = PhotoUrl,  // Add the photo URL
                    ListedDate = DateTime.UtcNow,
                    UserId = 1, // TODO: Get actual user ID from AuthService
                    JobType = SelectedCategory == "Jobs" ? JobType : null,
                    ServiceType = SelectedCategory == "Services" ? ServiceType : null,
                    RentalPeriod = SelectedCategory == "Rentals" ? RentalPeriod : null,
                    AvailableFrom = SelectedCategory == "Jobs" || SelectedCategory == "Rentals" ? AvailableFrom : null,
                    AvailableTo = SelectedCategory == "Rentals" ? AvailableTo : null
                };

                // Save the item using the item service
                var result = await _itemService.AddItemAsync(item);

                if (result)
                {
                    Debug.WriteLine("Item saved successfully");
                    await Shell.Current.DisplayAlert("Success", "Item posted successfully!", "OK");
                    // Navigate back to the previous page
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

                // Cleanup photo if save failed
                if (!string.IsNullOrEmpty(PhotoUrl))
                {
                    CleanupPhoto(PhotoUrl);
                    PhotoUrl = null;
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

    }
}
#endregion