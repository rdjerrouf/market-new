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

        private string title = string.Empty;
        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        private string description = string.Empty;
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
        private string _category = "For Sale";
        public string Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
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
            set => SetProperty(ref _isBusy, value);
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
            Debug.WriteLine("Upload button pressed");

            if (IsBusy)
            {
                Debug.WriteLine("Upload canceled - busy state");
                return;
            }

            try
            {
                IsBusy = true;
                Debug.WriteLine("Starting photo upload process...");

#if IOS || MACCATALYST
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

        if (OperatingSystem.IsIOSVersionAtLeast(14, 0) || OperatingSystem.IsMacCatalystVersionAtLeast(14, 0))
        {
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
        }
        else
        {
            Debug.WriteLine("Photos permission is not supported on this version of iOS or MacCatalyst.");
            await Shell.Current.DisplayAlert("Unsupported Version",
                "Photos access permission is not supported on this version of iOS or MacCatalyst.", "OK");
            return;
        }
#endif

                Debug.WriteLine("Launching media picker...");

                var photo = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Select a photo"
                });

                if (photo != null)
                {
                    Debug.WriteLine($"Photo selected: {photo.FileName}");

                    var localStorageDir = Path.Combine(FileSystem.AppDataDirectory, "ItemPhotos");
                    Directory.CreateDirectory(localStorageDir);

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
                await Shell.Current.DisplayAlert("Error", $"Failed to upload photo: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
                Debug.WriteLine("Photo upload process completed");
            }
        }

        private bool CanSaveItem()
        {
            return !IsBusy &&
                   !string.IsNullOrWhiteSpace(Title) &&
                   !string.IsNullOrWhiteSpace(Description) &&
                   Price > 0;
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