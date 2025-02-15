using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Market.Services;
using Market.DataAccess.Models;
using System.Diagnostics;
using System.Text;

namespace Market.ViewModels.AddItem
{
    public partial class RentalItemViewModel : ObservableObject
    {
        #region Fields and Constants
        private readonly IItemService _itemService;
        private readonly IAuthService _authService;

        // Validation constants
        private const int TITLE_MIN_LENGTH = 2;
        private const int DESCRIPTION_MIN_LENGTH = 4;
        private const decimal MIN_PRICE = 0.01m;
        private const decimal MAX_PRICE = 999999.99m;
        private const int MAX_PHOTOS = 3;
        private const int MIN_PHOTOS = 1;
        private const long MAX_PHOTO_SIZE = 5 * 1024 * 1024; // 5MB

        // List of available rental periods
        public List<string> RentalPeriods { get; } = new()
        {
            "per Day",
            "per Week",
            "per Month"
        };

        // Collection to store selected photo paths
        private List<string> _selectedPhotos = new();
        #endregion

        #region Observable Properties
        private string _title = string.Empty;
        public string Title
        {
            get => _title;
            set
            {
                if (SetProperty(ref _title, value))
                {
                    ValidateTitle();
                    OnPropertyChanged(nameof(CanSave));
                    SaveCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private string _description = string.Empty;
        public string Description
        {
            get => _description;
            set
            {
                Debug.WriteLine($"Description setter called with value: '{value}'");
                if (SetProperty(ref _description, value))
                {
                    Debug.WriteLine($"Description changed to: '{_description}'");
                    ValidateDescription();
                    OnPropertyChanged(nameof(CanSave));
                    SaveCommand.NotifyCanExecuteChanged();
                }
            }
        }




        private decimal _price;
        public decimal Price
        {
            get => _price;
            set
            {
                if (SetProperty(ref _price, value))
                {
                    ValidatePrice();
                    OnPropertyChanged(nameof(CanSave));
                    SaveCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private string _rentalPeriod = string.Empty;
        public string RentalPeriod
        {
            get => _rentalPeriod;
            set
            {
                SetProperty(ref _rentalPeriod, value);
            }
        }

        private DateTime _availableFrom = DateTime.Today;
        public DateTime AvailableFrom
        {
            get => _availableFrom;
            set
            {
                if (SetProperty(ref _availableFrom, value))
                {
                    ValidateDates();
                    OnPropertyChanged(nameof(CanSave));
                }
            }
        }

        private DateTime _availableTo = DateTime.Today;
        public DateTime AvailableTo
        {
            get => _availableTo;
            set
            {
                if (SetProperty(ref _availableTo, value))
                {
                    ValidateDates();
                    OnPropertyChanged(nameof(CanSave));
                }
            }
        }

        public List<string> SelectedPhotos
        {
            get => _selectedPhotos;
            set
            {
                if (SetProperty(ref _selectedPhotos, value))
                {
                    ValidatePhotos();
                    OnPropertyChanged(nameof(CanSave));
                    OnPropertyChanged(nameof(PhotoCount));
                    OnPropertyChanged(nameof(CanAddPhoto));
                }
            }
        }

        public int PhotoCount => SelectedPhotos.Count;
        public bool CanAddPhoto => PhotoCount < MAX_PHOTOS;

        #region Error Properties
        private string? _titleError;
        public string? TitleError
        {
            get => _titleError;
            set => SetProperty(ref _titleError, value);
        }

        private string? _descriptionError;
        public string? DescriptionError
        {
            get => _descriptionError;
            set
            {
                if (SetProperty(ref _descriptionError, value))
                {
                    OnPropertyChanged(nameof(HasDescriptionError));
                    OnPropertyChanged(nameof(CanSave));
                }
            }
        }

        private string? _priceError;
        public string? PriceError
        {
            get => _priceError;
            set => SetProperty(ref _priceError, value);
        }

        private string? _dateError;
        public string? DateError
        {
            get => _dateError;
            set => SetProperty(ref _dateError, value);
        }

        private string? _photoError;
        public string? PhotoError
        {
            get => _photoError;
            set => SetProperty(ref _photoError, value);
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }
        #endregion

        #region Computed Properties
        public bool HasTitleError => !string.IsNullOrEmpty(TitleError);
        public bool HasDescriptionError => !string.IsNullOrEmpty(DescriptionError);
        public bool HasPriceError => !string.IsNullOrEmpty(PriceError);
        public bool HasDateError => !string.IsNullOrEmpty(DateError);
        public bool HasPhotoError => !string.IsNullOrEmpty(PhotoError);

        public bool CanSave
        {
            get
            {
                var canSave = !IsBusy &&
                              !HasTitleError &&
                              !HasDescriptionError &&
                              !HasPriceError &&
                              !HasDateError &&
                              !HasPhotoError &&
                              PhotoCount >= MIN_PHOTOS;

                Debug.WriteLine("CanSave evaluation:");
                Debug.WriteLine($"- IsBusy: {IsBusy}");
                Debug.WriteLine($"- HasTitleError: {HasTitleError}");
                Debug.WriteLine($"- HasDescriptionError: {HasDescriptionError}");
                Debug.WriteLine($"- HasPriceError: {HasPriceError}");
                Debug.WriteLine($"- HasDateError: {HasDateError}");
                Debug.WriteLine($"- HasPhotoError: {HasPhotoError}");
                Debug.WriteLine($"- PhotoCount: {PhotoCount} (minimum: {MIN_PHOTOS})");
                Debug.WriteLine($"Final CanSave value: {canSave}");

                return canSave;
            }
        }

        #endregion

        public RentalItemViewModel(IItemService itemService, IAuthService authService)
        {
            try
            {
                Debug.WriteLine("Starting RentalItemViewModel initialization");

                _itemService = itemService ?? throw new ArgumentNullException(nameof(itemService));
                _authService = authService ?? throw new ArgumentNullException(nameof(authService));

                if (RentalPeriods.Count > 0)
                {
                    RentalPeriod = RentalPeriods[0];
                }

                // Add these lines to perform initial validation
                ValidateTitle();
                ValidateDescription();
                ValidatePrice();
                ValidateDates();
                ValidatePhotos();

                Debug.WriteLine("RentalItemViewModel initialization completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in RentalItemViewModel constructor: {ex}");
                throw;
            }
        }

        #region Validation Methods
        private bool ValidateTitle()
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                TitleError = "Please enter an item title";
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

        private bool ValidateDescription()
        {
            Debug.WriteLine($"Validating description. Current value: '{Description}'");
            Debug.WriteLine($"Description length before trim: {Description?.Length ?? 0}");
            Debug.WriteLine($"Description length after trim: {Description?.Trim().Length ?? 0}");

            if (string.IsNullOrWhiteSpace(Description))
            {
                DescriptionError = "Please enter an item description";
                Debug.WriteLine("Validation failed: Description is empty or whitespace");
                return false;
            }

            var trimmedLength = Description.Trim().Length;
            if (trimmedLength < DESCRIPTION_MIN_LENGTH)
            {
                DescriptionError = $"Description must be at least {DESCRIPTION_MIN_LENGTH} characters";
                Debug.WriteLine($"Validation failed: Description length {trimmedLength} is less than minimum {DESCRIPTION_MIN_LENGTH}");
                return false;
            }

            DescriptionError = null;
            Debug.WriteLine("Description validation passed");
            return true;
        }

        private bool ValidatePrice()
        {
            Debug.WriteLine($"Validating price: {Price}");

            if (Price < MIN_PRICE)
            {
                Debug.WriteLine($"Price {Price} is below minimum {MIN_PRICE}");
                PriceError = "Price must be greater than zero";
                return false;
            }

            if (Price > MAX_PRICE)
            {
                Debug.WriteLine($"Price {Price} exceeds maximum {MAX_PRICE}");
                PriceError = $"Price cannot exceed {MAX_PRICE:C}";
                return false;
            }

            Debug.WriteLine("Price validation passed");
            PriceError = null;
            return true;
        }

        private bool ValidateDates()
        {
            if (AvailableFrom < DateTime.Today)
            {
                DateError = "Start date cannot be in the past";
                return false;
            }

            if (AvailableTo < AvailableFrom)
            {
                DateError = "End date must be after start date";
                return false;
            }

            DateError = null;
            return true;
        }

        private bool ValidatePhotos()
        {
            if (PhotoCount < MIN_PHOTOS)
            {
                PhotoError = $"Please add at least {MIN_PHOTOS} photo";
                return false;
            }

            if (PhotoCount > MAX_PHOTOS)
            {
                PhotoError = $"Maximum {MAX_PHOTOS} photos allowed";
                return false;
            }

            PhotoError = null;
            return true;
        }

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

        private bool ValidateAll()
        {
            Debug.WriteLine("Running ValidateAll");

            var titleValid = ValidateTitle();
            Debug.WriteLine($"- Title validation: {titleValid}");

            var descriptionValid = ValidateDescription();
            Debug.WriteLine($"- Description validation: {descriptionValid}");

            var priceValid = ValidatePrice();
            Debug.WriteLine($"- Price validation: {priceValid}");

            var datesValid = ValidateDates();
            Debug.WriteLine($"- Dates validation: {datesValid}");

            var photosValid = ValidatePhotos();
            Debug.WriteLine($"- Photos validation: {photosValid}");

            var allValid = titleValid &&
                           descriptionValid &&
                           priceValid &&
                           datesValid &&
                           photosValid;

            Debug.WriteLine($"ValidateAll result: {allValid}");

            return allValid;
        }
        #endregion

        #region Commands
        [RelayCommand]
        public async Task AddPhoto()
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
                    var newPhotos = new List<string>(SelectedPhotos) { photoPath };
                    SelectedPhotos = newPhotos;
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

        [RelayCommand]
        private void RemovePhoto(string photo)
        {
            if (SelectedPhotos.Contains(photo))
            {
                var newList = new List<string>(SelectedPhotos);
                newList.Remove(photo);
                SelectedPhotos = newList;
                Debug.WriteLine($"Photo removed: {photo}");
            }
        }
        private static async Task<string?> SavePhoto(FileResult photo)
        {
            try
            {
                var photosDir = Path.Combine(FileSystem.CacheDirectory, "Photos");
                if (!Directory.Exists(photosDir))
                    Directory.CreateDirectory(photosDir);

                var fileName = $"rental_photo_{Guid.NewGuid()}{Path.GetExtension(photo.FileName)}";
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
                throw;
            }
        }

        [RelayCommand(CanExecute = nameof(CanSave))]
        private async Task Save()
        {
            Debug.WriteLine("Save command executed");
            if (IsBusy)
            {
                Debug.WriteLine("Save canceled - IsBusy is true");
                return;
            }

            try
            {
                IsBusy = true;
                Debug.WriteLine("Starting rental item save process");

                var isValid = ValidateAll();
                Debug.WriteLine($"ValidateAll result: {isValid}");

                if (!isValid)
                {
                    Debug.WriteLine("Validation failed during save");
                    return;
                }

                var item = new Item
                {
                    Title = Title,
                    Description = BuildFullDescription(),
                    Price = Price,
                    Category = "Rentals",
                    RentalPeriod = RentalPeriod,
                    AvailableFrom = AvailableFrom,
                    AvailableTo = AvailableTo,
                    ListedDate = DateTime.UtcNow,
                    UserId = await _authService.GetCurrentUserIdAsync(),
                    PhotoUrl = string.Join(";", SelectedPhotos)
                };

                Debug.WriteLine($"Attempting to save rental item: {item.Title}");
                var saveResult = await _itemService.AddItemAsync(item);

                if (saveResult)
                {
                    Debug.WriteLine("Save successful");
                    await Shell.Current.DisplayAlert("Success", "Your rental item has been posted!", "OK");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    Debug.WriteLine("Save failed");
                    await Shell.Current.DisplayAlert("Error", "Failed to post rental item", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Save error: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                await Shell.Current.DisplayAlert("Error", "An error occurred while saving", "OK");
            }
            finally
            {
                IsBusy = false;
                Debug.WriteLine("Save process completed");
            }
        }
        #endregion

        #region Helper Methods
        private string BuildFullDescription()
        {
            var fullDescription = new StringBuilder();
            fullDescription.AppendLine(Description);
            fullDescription.AppendLine();
            fullDescription.AppendLine($"Available From: {AvailableFrom:d}");
            fullDescription.AppendLine($"Available To: {AvailableTo:d}");
            fullDescription.AppendLine($"Rate: {Price:C} {RentalPeriod}");

            return fullDescription.ToString();
        }
        #endregion
    }
}
#endregion