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

        [ObservableProperty]
        private string _title = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private decimal _price;

        [ObservableProperty]
        private string _category = "For Sale";

        [ObservableProperty]
        private string? _photoUrl;

        [ObservableProperty]
        private bool _isBusy;

        public PostItemViewModel(IItemService itemService, IAuthService authService)
        {
            _itemService = itemService;
            _authService = authService;
            Debug.WriteLine("PostItemViewModel initialized");
        }

        [RelayCommand]
        private async Task UploadPhotoAsync()
        {
            await Shell.Current.DisplayAlert("Debug", "Upload method called!", "OK");
            if (IsBusy)
            {
                Debug.WriteLine("Upload canceled - busy state");
                return;
            }

            try
            {
                IsBusy = true;
                Debug.WriteLine("Starting photo upload process...");

                // Request permissions
                var storageStatus = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
                var photosStatus = await Permissions.CheckStatusAsync<Permissions.Photos>();

                if (storageStatus != PermissionStatus.Granted)
                {
                    storageStatus = await Permissions.RequestAsync<Permissions.StorageRead>();
                }

                if (photosStatus != PermissionStatus.Granted)
                {
                    photosStatus = await Permissions.RequestAsync<Permissions.Photos>();
                }

                if (storageStatus != PermissionStatus.Granted || photosStatus != PermissionStatus.Granted)
                {
                    await Shell.Current.DisplayAlert("Permission Required",
                        "Storage and Photos access permissions are required to upload photos.", "OK");
                    return;
                }

                var photo = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Select a photo"
                });

                if (photo != null)
                {
                    var localStorageDir = Path.Combine(FileSystem.AppDataDirectory, "ItemPhotos");
                    Directory.CreateDirectory(localStorageDir);

                    var fileName = $"item_photo_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(photo.FileName)}";
                    var localFilePath = Path.Combine(localStorageDir, fileName);

                    using (var sourceStream = await photo.OpenReadAsync())
                    using (var destinationStream = File.OpenWrite(localFilePath))
                    {
                        await sourceStream.CopyToAsync(destinationStream);
                    }

                    PhotoUrl = localFilePath;
                    await Shell.Current.DisplayAlert("Success", "Photo uploaded successfully!", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in photo upload: {ex}");
                await Shell.Current.DisplayAlert("Error", $"Failed to upload photo: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
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