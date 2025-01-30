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
    /// This implementation is AOT-compatible for WinRT scenarios.
    /// </summary>
    public partial class PostItemViewModel : ObservableObject
    {
        // Services for handling data operations
        private readonly IItemService _itemService;
        private readonly IAuthService _authService;

        // Private backing fields for properties
        private string _title = string.Empty;
        private string _description = string.Empty;
        private decimal _price;
        private string _category = "For Sale"; // Default category
        private string? _photoUrl;
        private bool _isBusy;

        #region Properties
        /// <summary>
        /// Title of the item being posted
        /// </summary>
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        /// <summary>
        /// Detailed description of the item
        /// </summary>
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        /// <summary>
        /// Price of the item
        /// </summary>
        public decimal Price
        {
            get => _price;
            set => SetProperty(ref _price, value);
        }

        /// <summary>
        /// Category of the item (e.g., "For Sale", "Services")
        /// </summary>
        public string Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
        }

        /// <summary>
        /// Local path to the item's photo
        /// </summary>
        public string? PhotoUrl
        {
            get => _photoUrl;
            set => SetProperty(ref _photoUrl, value);
        }

        /// <summary>
        /// Flag indicating if the ViewModel is performing an operation
        /// </summary>
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }
        #endregion

        /// <summary>
        /// Constructor - initializes required services
        /// </summary>
        /// <param name="itemService">Service for item operations</param>
        /// <param name="authService">Service for authentication</param>
        public PostItemViewModel(IItemService itemService, IAuthService authService)
        {
            _itemService = itemService;
            _authService = authService;
            Debug.WriteLine("PostItemViewModel initialized");
        }

        /// <summary>
        /// Determines if an item can be saved based on required fields
        /// </summary>
        /// <returns>True if all required fields are filled and not busy</returns>
        private bool CanSaveItem()
        {
            return !IsBusy &&
                   !string.IsNullOrWhiteSpace(Title) &&
                   !string.IsNullOrWhiteSpace(Description) &&
                   Price > 0;
        }

        /// <summary>
        /// Handles saving the item to the database
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
                    Status = Category,
                    ListedDate = DateTime.UtcNow,
                    UserId = 1 // TODO: Get actual user ID from AuthService
                };

                // Convert photo URL to relative path for storage
                if (!string.IsNullOrEmpty(PhotoUrl))
                {
                    var relativePath = PhotoUrl.Replace(FileSystem.AppDataDirectory, string.Empty);
                    item.PhotoUrl = relativePath;
                }

                // Attempt to save the item
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
        /// Handles photo selection and storage in the app's local storage
        /// Includes permission checking and file management
        /// </summary>
        [RelayCommand]
        private async Task UploadPhotoAsync()
        {
            if (IsBusy)
            {
                Debug.WriteLine("Upload canceled - busy state");
                return;
            }

            try
            {
                IsBusy = true;
                Debug.WriteLine("Starting photo upload process...");

                // Check storage permission first
                var storageStatus = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
                Debug.WriteLine($"Storage permission status: {storageStatus}");

                if (storageStatus != PermissionStatus.Granted)
                {
                    Debug.WriteLine("Requesting storage permission...");
                    storageStatus = await Permissions.RequestAsync<Permissions.StorageRead>();

                    if (storageStatus != PermissionStatus.Granted)
                    {
                        Debug.WriteLine("Storage permission denied");
                        await Shell.Current.DisplayAlert("Permission Required",
                            "Storage access permission is required to upload photos.", "OK");
                        return;
                    }
                }

                // Check photos permission
                var photosStatus = await Permissions.CheckStatusAsync<Permissions.Photos>();
                Debug.WriteLine($"Photos permission status: {photosStatus}");

                if (photosStatus != PermissionStatus.Granted)
                {
                    Debug.WriteLine("Requesting photos permission...");
                    photosStatus = await Permissions.RequestAsync<Permissions.Photos>();

                    if (photosStatus != PermissionStatus.Granted)
                    {
                        Debug.WriteLine("Photos permission denied");
                        await Shell.Current.DisplayAlert("Permission Required",
                            "Photos access permission is required to upload photos.", "OK");
                        return;
                    }
                }

                Debug.WriteLine("Launching media picker...");

                var photo = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Select a photo"
                });

                if (photo != null)
                {
                    Debug.WriteLine($"Photo selected: {photo.FileName}");

                    // Create app's local storage directory
                    var localStorageDir = Path.Combine(FileSystem.AppDataDirectory, "ItemPhotos");
                    Directory.CreateDirectory(localStorageDir);

                    // Generate unique filename
                    var fileName = $"item_photo_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(photo.FileName)}";
                    var localFilePath = Path.Combine(localStorageDir, fileName);

                    Debug.WriteLine($"Copying to local storage: {localFilePath}");

                    using (var sourceStream = await photo.OpenReadAsync())
                    using (var destinationStream = File.OpenWrite(localFilePath))
                    {
                        await sourceStream.CopyToAsync(destinationStream);
                    }

                    PhotoUrl = localFilePath;
                    Debug.WriteLine($"Photo saved successfully. PhotoUrl set to: {PhotoUrl}");

                    await Shell.Current.DisplayAlert("Success", "Photo uploaded successfully!", "OK");
                }
                else
                {
                    Debug.WriteLine("No photo selected");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in photo upload: {ex}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                await Shell.Current.DisplayAlert("Error",
                    $"Failed to upload photo: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
                Debug.WriteLine("Photo upload process completed");
            }
        }
    }
}