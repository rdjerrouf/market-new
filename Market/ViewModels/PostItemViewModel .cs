using Market.DataAccess.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Market.Services;
using System.Diagnostics;

namespace Market.ViewModels
{
    public partial class PostItemViewModel : ObservableObject
    {
        private readonly IItemService _itemService;
        private readonly IAuthService _authService;

        
        
       
        
        
       

        // Add validation message properties
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
            set => SetProperty(ref _descriptionError, value);
        }

        private string? _priceError;
        public string? PriceError
        {
            get => _priceError;
            set => SetProperty(ref _priceError, value);
        }

        private string? _categoryError;
        public string? CategoryError
        {
            get => _categoryError;
            set => SetProperty(ref _categoryError, value);
        }

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


        private decimal price;
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

        private string? _photoUrl;
        public string? PhotoUrl
        {
            get => _photoUrl;
            set => SetProperty(ref _photoUrl, value);
            
        }

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

        // Validation methods
        private bool ValidateTitle()
        {
            const int minLength = 3;
            const int maxLength = 100;

            if (string.IsNullOrWhiteSpace(Title))
            {
                TitleError = "Title is required";
                return false;
            }

            if (Title.Length < minLength)
            {
                TitleError = $"Title must be at least {minLength} characters";
                return false;
            }

            if (Title.Length > maxLength)
            {
                TitleError = $"Title cannot exceed {maxLength} characters";
                return false;
            }

            TitleError = null;
            return true;
        }

        private bool ValidateDescription()
        {
            const int minLength = 10;
            const int maxLength = 1000;

            if (string.IsNullOrWhiteSpace(Description))
            {
                DescriptionError = "Description is required";
                return false;
            }

            if (Description.Length < minLength)
            {
                DescriptionError = $"Description must be at least {minLength} characters";
                return false;
            }

            if (Description.Length > maxLength)
            {
                DescriptionError = $"Description cannot exceed {maxLength} characters";
                return false;
            }

            DescriptionError = null;
            return true;
        }

        private bool ValidatePrice()
        {
            const decimal minPrice = 0.01m;
            const decimal maxPrice = 999999.99m;

            if (Price < minPrice)
            {
                PriceError = "Price must be greater than zero";
                return false;
            }

            if (Price > maxPrice)
            {
                PriceError = $"Price cannot exceed {maxPrice:C}";
                return false;
            }

            PriceError = null;
            return true;
        }

        private bool ValidateCategory()
        {
            var validCategories = new[] { "For Sale", "Jobs", "Services", "Rentals" };

            if (string.IsNullOrWhiteSpace(Category))
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

        // Update CanSaveItem to use all validations
        private bool CanSaveItem()
        {
            var isValid = !IsBusy &&
                         ValidateTitle() &&
                         ValidateDescription() &&
                         ValidatePrice() &&
                         ValidateCategory();

            Debug.WriteLine($"CanSaveItem check - " +
                           $"IsBusy: {IsBusy}, " +
                           $"Title valid: {TitleError == null}, " +
                           $"Description valid: {DescriptionError == null}, " +
                           $"Price valid: {PriceError == null}, " +
                           $"Category valid: {CategoryError == null}, " +
                           $"Final result: {isValid}");

            return isValid;
        }

        public PostItemViewModel(IItemService itemService, IAuthService authService)
        {
            _itemService = itemService;
            _authService = authService;
            Debug.WriteLine("PostItemViewModel initialized");
        }

        [RelayCommand]
        private async Task UploadPhotoAsync()
        {
            if (IsBusy) return;

            string? tempPhotoPath = null;
            try
            {
                IsBusy = true;
                Debug.WriteLine("Starting photo upload");

#if IOS || MACCATALYST
                if (!await CheckPhotoPermissions()) return;
#endif

                var photo = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Select a photo"
                });

                if (photo == null) return;

                Debug.WriteLine($"Photo selected: {photo.FileName}");

                if (!await ValidatePhotoAsync(photo)) return;

                var localStorageDir = Path.Combine(FileSystem.AppDataDirectory, "ItemPhotos");
                Directory.CreateDirectory(localStorageDir);

                var fileName = $"item_photo_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(photo.FileName)}";
                tempPhotoPath = Path.Combine(localStorageDir, fileName);

                using (var sourceStream = await photo.OpenReadAsync())
                using (var destinationStream = File.OpenWrite(tempPhotoPath))
                {
                    await sourceStream.CopyToAsync(destinationStream);
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
        /// Cleans up photo files from failed uploads
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
        /// Validates photo file size and format
        /// </summary>
        private async Task<bool> ValidatePhotoAsync(FileResult photo)
        {
            Debug.WriteLine($"Validating photo: {photo.FileName}");

            // Check file size (max 5MB)
            const long maxFileSize = 5 * 1024 * 1024;
            using var stream = await photo.OpenReadAsync();
            if (stream.Length > maxFileSize)
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
        private async Task<bool> CheckPhotoPermissions()
        {
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


        [RelayCommand(CanExecute = nameof(CanSaveItem))]
        private async Task SaveItem()  // Changed from SaveItemAsync to SaveItem
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                Debug.WriteLine("Starting item save process...");

                var item = new Item
                {
                    Title = Title,
                    Description = Description,
                    Price = Price,
                    Status = Category,
                    ListedDate = DateTime.UtcNow,
                    UserId = 1 // TODO: Get actual user ID from AuthService
                };

                if (!string.IsNullOrEmpty(PhotoUrl))
                {
                    var relativePath = PhotoUrl.Replace(FileSystem.AppDataDirectory, string.Empty);
                    item.PhotoUrl = relativePath;
                }

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
    }
}