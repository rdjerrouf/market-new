using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Market.Services;
using Market.DataAccess.Models;
using System.Diagnostics;

namespace Market.ViewModels.AddItem
{
    /// <summary>
    /// ViewModel for rental listings with specialized handling for rental periods,
    /// availability dates, and pricing. Manages the creation and validation
    /// of rental offerings in the marketplace.
    /// </summary>
    public partial class RentalItemViewModel : ObservableObject
    {
        #region Fields and Constants

        private readonly IItemService _itemService;
        private readonly IAuthService _authService;

        // Validation constants
        private const int TITLE_MIN_LENGTH = 5;
        private const int DESCRIPTION_MIN_LENGTH = 20;
        private const decimal MIN_PRICE = 0.01m;
        private const decimal MAX_PRICE = 999999.99m;

        // Rental period options
        public List<string> RentalPeriodOptions { get; } 
            = ["Per Day", "Per Week", "Per Month", "Per Year"];

        #endregion

        #region Observable Properties

        /// <summary>
        /// Title of the rental item
        /// </summary>

        private string title=String.Empty;
        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        /// <summary>
        /// Description of the rental item
        /// </summary>

        private string description=String.Empty;
        public string Description
        {
            get => description;
            set => SetProperty(ref description, value);
        }

        /// <summary>
        /// Rental price
        /// </summary>
      
        private decimal price;
        public decimal Price
        {
            get => price;
            set => SetProperty(ref price, value);
        }

        /// <summary>
        /// Selected rental period
        /// </summary>
        
        private string rentalPeriod = String.Empty;
        public string RentalPeriod
        {
            get => rentalPeriod;
            set => SetProperty(ref rentalPeriod, value);
        }

        /// <summary>
        /// Start date of rental availability
        /// </summary>
        
        private DateTime availableFrom = DateTime.Today;
        public DateTime AvailableFrom
        {
            get => availableFrom;
            set => SetProperty(ref availableFrom, value);
        }

        /// <summary>
        /// End date of rental availability
        /// </summary>

        private DateTime availableTo = DateTime.Today.AddMonths(1);
        public DateTime AvailableTo
        {
            get => availableTo;
            set => SetProperty(ref availableTo, value);
        }
        /// <summary>
        /// Path to uploaded photo
        /// </summary>

        private string? photoUrl = String.Empty;
        public string? PhotoUrl
        {
            get => photoUrl;
            set => SetProperty(ref photoUrl, value);
        }

        /// <summary>
        /// Indicates if an operation is in progress
        /// </summary>
        private bool isBusy;
        
        public bool IsBusy
        {
            get => isBusy;
            set => SetProperty(ref isBusy, value);
        }

        #endregion

        #region Error Properties

        /// <summary>
        /// Error message for title validation
        /// </summary>
        private string? titleError = String.Empty;
        public string? TitleError
        {
            get => titleError;
            set => SetProperty(ref titleError, value);
        }

        /// <summary>
        /// Error message for description validation
        /// </summary>
        
        private string? descriptionError = String.Empty;
        public string? DescriptionError
        {
            get => descriptionError;
            set => SetProperty(ref descriptionError, value);
        }

        /// <summary>
        /// Error message for price validation
        /// </summary>
       
        private string? priceError = String.Empty;
        public string? PriceError
        {
            get => priceError;
            set => SetProperty(ref priceError, value);
        }

        /// <summary>
        /// Error message for date validation
        /// </summary>
       
        private string? dateError = String.Empty;
        public string? DateError
        {
            get => dateError;
            set => SetProperty(ref dateError, value);
        }

        #endregion

        #region Computed Properties

        /// <summary>
        /// Indicates if the form can be submitted
        /// </summary>
        public bool CanSave => !IsBusy &&
                              !HasTitleError &&
                              !HasDescriptionError &&
                              !HasPriceError &&
                              !HasDateError;

        /// <summary>
        /// UI state indicators for error messages
        /// </summary>
        public bool HasTitleError => !string.IsNullOrEmpty(TitleError);
        public bool HasDescriptionError => !string.IsNullOrEmpty(DescriptionError);
        public bool HasPriceError => !string.IsNullOrEmpty(PriceError);
        public bool HasDateError => !string.IsNullOrEmpty(DateError);
        public bool IsNotBusy => !IsBusy;
        public bool HasPhoto => !string.IsNullOrEmpty(PhotoUrl);
        public bool HasNoPhoto => string.IsNullOrEmpty(PhotoUrl);

        #endregion

        /// <summary>
        /// Initializes a new instance of RentalItemViewModel
        /// </summary>
        public RentalItemViewModel(IItemService itemService, IAuthService authService)
        {
            _itemService = itemService ?? throw new ArgumentNullException(nameof(itemService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));

            // Set default values
            RentalPeriod = RentalPeriodOptions[0];

            Debug.WriteLine("RentalItemViewModel initialized");
        }

        #region Validation Methods

        /// <summary>
        /// Validates the rental item title
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
            return true;
        }

        /// <summary>
        /// Validates the rental item description
        /// </summary>
        private bool ValidateDescription()
        {
            if (string.IsNullOrWhiteSpace(Description))
            {
                DescriptionError = "Please describe your rental item";
                return false;
            }

            if (Description.Length < DESCRIPTION_MIN_LENGTH)
            {
                DescriptionError = $"Description must be at least {DESCRIPTION_MIN_LENGTH} characters";
                return false;
            }

            DescriptionError = null;
            return true;
        }

        /// <summary>
        /// Validates the rental price
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
            return true;
        }

        /// <summary>
        /// Validates the rental availability dates
        /// </summary>
        private bool ValidateDates()
        {
            if (AvailableFrom > AvailableTo)
            {
                DateError = "Available from date must be before available to date";
                return false;
            }

            if (AvailableFrom < DateTime.Today)
            {
                DateError = "Available from date cannot be in the past";
                return false;
            }

            DateError = null;
            return true;
        }

        #endregion

        #region Photo Handling Methods

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

                var photo = await MediaPicker.PickPhotoAsync();
                if (photo == null) return;

                // Validate photo
                if (!await ValidatePhoto(photo)) return;

                // Save photo
                var photoPath = await SavePhoto(photo);
                if (photoPath != null)
                {
                    PhotoUrl = photoPath;
                    await Shell.Current.DisplayAlert("Success", "Photo uploaded", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Photo upload error: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to upload photo", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Validates the uploaded photo
        /// </summary>
        private static async Task<bool> ValidatePhoto(FileResult photo)
        {
            const long MAX_PHOTO_SIZE = 5 * 1024 * 1024; // 5MB

            using var stream = await photo.OpenReadAsync();
            if (stream.Length > MAX_PHOTO_SIZE)
            {
                await Shell.Current.DisplayAlert("Error", "Photo must be less than 5MB", "OK");
                return false;
            }

            var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();
            if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
            {
                await Shell.Current.DisplayAlert("Error", "Please select a JPG or PNG file", "OK");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Saves the photo to local storage
        /// </summary>
        private static async Task<string?> SavePhoto(FileResult photo)
        {
            try
            {
                var localPath = Path.Combine(FileSystem.AppDataDirectory,
                    $"rental_photo_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(photo.FileName)}");

                using var sourceStream = await photo.OpenReadAsync();
                using var destinationStream = File.OpenWrite(localPath);
                await sourceStream.CopyToAsync(destinationStream);

                return localPath;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Photo save error: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Save Command

        /// <summary>
        /// Saves the rental item to the marketplace
        /// </summary>
        [RelayCommand]
        private async Task Save()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                Debug.WriteLine("Starting rental item save process");

                // Validate all fields
                if (!ValidateAll())
                {
                    return;
                }

                var rentalItem = new Item
                {
                    Title = Title,
                    Description = Description,
                    Price = Price,
                    Category = "Rentals",
                    RentalPeriod = RentalPeriod,
                    AvailableFrom = AvailableFrom,
                    AvailableTo = AvailableTo,
                    PhotoUrl = PhotoUrl,
                    ListedDate = DateTime.UtcNow,
                    UserId = await _authService.GetCurrentUserIdAsync()
                };

                Debug.WriteLine($"Saving rental item: {rentalItem.Title}, {rentalItem.Price} {rentalItem.RentalPeriod}");
                var result = await _itemService.AddItemAsync(rentalItem);

                if (result)
                {
                    await Shell.Current.DisplayAlert("Success", "Your rental has been posted!", "OK");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to post rental", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Save error: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "An error occurred while saving", "OK");
            }
            finally
            {
                IsBusy = false;
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
                   && ValidatePrice()
                   && ValidateDates();
        }

        #endregion
    }
}