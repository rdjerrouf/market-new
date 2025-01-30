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
                    Status = Category,
                    ListedDate = DateTime.UtcNow,
                    UserId = 1 // TODO: Get actual user ID from AuthService
                };

                // Add this block right here, before saving to database
                if (!string.IsNullOrEmpty(PhotoUrl))
                {
                    // Convert to relative path for storage
                    var relativePath = PhotoUrl.Replace(FileSystem.AppDataDirectory, string.Empty);
                    item.PhotoUrl = relativePath;
                }

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
        /// <summary>
        /// Handles photo selection and storage in the app's local storage
        /// </summary>
        [RelayCommand]
        private async Task UploadPhotoAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                Debug.WriteLine("Starting photo upload process...");

                // Check if permission is granted
                var status = await Permissions.CheckStatusAsync<Permissions.Photos>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.Photos>();
                    if (status != PermissionStatus.Granted)
                    {
                        await Shell.Current.DisplayAlert("Permission Required",
                            "Photo access permission is required to upload photos.", "OK");
                        return;
                    }
                }

                // Open photo picker
                var photo = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Select a photo"
                });

                if (photo != null)
                {
                    // Create local storage directory if it doesn't exist
                    var localStorageDir = Path.Combine(FileSystem.AppDataDirectory, "ItemPhotos");
                    if (!Directory.Exists(localStorageDir))
                    {
                        Directory.CreateDirectory(localStorageDir);
                    }

                    // Generate unique filename
                    var fileName = $"item_photo_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(photo.FileName)}";
                    var localFilePath = Path.Combine(localStorageDir, fileName);

                    // Copy the selected photo to app's local storage
                    using Stream sourceStream = await photo.OpenReadAsync();
                    using FileStream localFileStream = File.OpenWrite(localFilePath);
                    await sourceStream.CopyToAsync(localFileStream);

                    // Update the PhotoUrl property
                    PhotoUrl = localFilePath;
                    Debug.WriteLine($"Photo saved locally: {PhotoUrl}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Photo upload error: {ex.Message}");
                await Shell.Current.DisplayAlert("Error",
                    "Failed to upload photo. Please try again.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Cleans up temporary photo files when no longer needed
        /// </summary>
        private void CleanupPhotos()
        {
            try
            {
                var localStorageDir = Path.Combine(FileSystem.AppDataDirectory, "ItemPhotos");
                if (Directory.Exists(localStorageDir))
                {
                    // Delete files older than 24 hours
                    var files = Directory.GetFiles(localStorageDir);
                    foreach (var file in files)
                    {
                        var fileInfo = new FileInfo(file);
                        if ((DateTime.Now - fileInfo.CreationTime).TotalHours > 24)
                        {
                            File.Delete(file);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error cleaning up photos: {ex.Message}");
            }
        }
    }
}