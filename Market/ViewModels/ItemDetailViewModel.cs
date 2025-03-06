using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Market.DataAccess.Models;
using Market.Market.DataAccess.Models;
using Market.Services;
using Market.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Market.ViewModels
{
    [QueryProperty(nameof(Item), "Item")]
    [QueryProperty(nameof(ItemId), "ItemId")]
    public partial class ItemDetailViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly IItemService _itemService;

        public ItemDetailViewModel(IAuthService authService, IItemService itemService)
        {
            _authService = authService;
            _itemService = itemService;
            PhotoItems = new ObservableCollection<PhotoViewModel>();
        }

        [ObservableProperty]
        private Item? item;

        [ObservableProperty]
        private int itemId;

        [ObservableProperty]
        private string title = string.Empty;

        [ObservableProperty]
        private bool isOwner;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string sellerName = string.Empty;

        [ObservableProperty]
        private string sellerProfilePicture = string.Empty;

        [ObservableProperty]
        private ObservableCollection<PhotoViewModel> photoItems;

        [ObservableProperty]
        private string statusColor = "#28a745"; // Default green

        [ObservableProperty]
        private bool isFavorite;

        [ObservableProperty]
        private bool isSaleItem;

        [ObservableProperty]
        private bool isRentalItem;

        [ObservableProperty]
        private bool isJobItem;

        [ObservableProperty]
        private bool isServiceItem;

        public class PhotoViewModel
        {
            public string ImageUrl { get; set; } = string.Empty;
        }

        public async Task InitializeAsync()
        {
            if (Item != null)
            {
                await LoadItemDetailsAsync(Item.Id);
            }
            else if (ItemId > 0)
            {
                await LoadItemDetailsAsync(ItemId);
            }
        }

        private async Task LoadItemDetailsAsync(int id)
        {
            try
            {
                IsLoading = true;

                var loadedItem = await _itemService.GetItemAsync(id);

                if (loadedItem != null)
                {
                    Item = loadedItem;
                    await OnItemSetAsync(); // Fix: Call the correct async method
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading item: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to load item details", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task OnItemSetAsync()
        {
            if (Item == null) return;

            Title = Item.Title;

            // Check if current user is the owner
            var currentUserId = await GetCurrentUserId();
            IsOwner = Item.PostedByUserId == currentUserId;

            // Set seller info
            SellerName = Item.PostedByUser?.DisplayName ?? "Unknown Seller";
            SellerProfilePicture = Item.PostedByUser?.ProfilePicture ?? "default_avatar.png";

            // Set status color
            StatusColor = Item.Status switch
            {
                ItemStatus.Active => "#28a745",      // Green
                ItemStatus.Sold => "#dc3545",        // Red
                ItemStatus.Rented => "#fd7e14",      // Orange
                ItemStatus.Unavailable => "#6c757d", // Gray
                _ => "#28a745"                       // Default Green
            };

            // Determine item type for conditional display
            DetermineItemType();

            // Check if item is in user's favorites
            await CheckIfFavorite();

            // Load photos
            LoadPhotos();
        }

        private void LoadPhotos()
        {
            PhotoItems.Clear();

            if (Item?.Photos?.Any() == true)
            {
                foreach (var photo in Item.Photos.OrderBy(p => p.IsPrimaryPhoto ? 0 : p.DisplayOrder))
                {
                    PhotoItems.Add(new PhotoViewModel { ImageUrl = photo.PhotoUrl });
                }
            }
            else if (!string.IsNullOrEmpty(Item?.PhotoUrl))
            {
                PhotoItems.Add(new PhotoViewModel { ImageUrl = Item.PhotoUrl });
            }
            else
            {
                PhotoItems.Add(new PhotoViewModel { ImageUrl = "placeholder_image.png" });
            }
        }

        private void DetermineItemType()
        {
            if (Item == null) return;

            IsSaleItem = Item.ForSaleCategory != null;
            IsRentalItem = Item.ForRentCategory != null || !string.IsNullOrEmpty(Item.RentalPeriod);
            IsJobItem = Item.JobCategory != null || !string.IsNullOrEmpty(Item.JobType);
            IsServiceItem = Item.ServiceCategory != null || !string.IsNullOrEmpty(Item.ServiceType);
        }

        private async Task CheckIfFavorite()
        {
            if (Item == null) return;

            try
            {
                var currentUserId = await GetCurrentUserId();
                IsFavorite = await Task.Run(() => Item.FavoritedByUsers.Any(u => u.Id == currentUserId));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking favorite status: {ex.Message}");
            }
        }


        [RelayCommand]
        private async Task ViewSellerProfile()
        {
            if (Item?.PostedByUserId <= 0) return;

            await Shell.Current.GoToAsync($"{nameof(UserProfilePage)}?UserId={Item.PostedByUserId}");
        }

        [RelayCommand]
        private async Task ReportItem()
        {
            if (Item == null) return;

            await Shell.Current.GoToAsync($"{nameof(ReportItemPage)}?ItemId={Item.Id}");
        }

        [RelayCommand]
        private async Task ContactSeller()
        {
            if (Item == null) return;

            try
            {
                Debug.WriteLine($"Contacting seller for item: {Item.Id}");
                // For now, just show a message. Later you can implement messaging
                await Shell.Current.DisplayAlert("Contact", "This feature is coming soon!", "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in ContactSeller: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Unable to contact seller at this time", "OK");
            }
        }

        [RelayCommand]
        private async Task ToggleFavorite()
        {
            if (Item == null) return;

            try
            {
                var currentUserId = await GetCurrentUserId(); // Fix: Await the task to get the actual user ID

                // Use your existing methods to toggle favorites
                if (IsFavorite)
                {
                    // For example, if you have a RemoveFavorite method instead
                    await _itemService.RemoveFavoriteAsync(currentUserId, Item.Id); // Fix: Correct the order of parameters
                    IsFavorite = false;
                }
                else
                {
                    // For example, if you have an AddFavorite method instead
                    await _itemService.AddFavoriteAsync(currentUserId, Item.Id); // Fix: Correct the order of parameters
                    IsFavorite = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error toggling favorite: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Unable to update favorites at this time", "OK");
            }
        }



        [RelayCommand]
        private void ShareItem()
        {
            if (Item == null) return;

            try
            {
                // This would use the Share API in a real implementation
                Share.RequestAsync(new ShareTextRequest
                {
                    Title = "Share Item",
                    Text = $"Check out this item: {Item.Title}",
                    Uri = $"https://yourapp.com/items/{Item.Id}"
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error sharing item: {ex.Message}");
                Shell.Current.DisplayAlert("Error", "Unable to share this item", "OK");
            }
        }

        [RelayCommand]
        private async Task ViewOnMap()
        {
            if (Item == null || !Item.HasLocation) return;

            try
            {
                await Shell.Current.GoToAsync($"{nameof(ItemMapPage)}?ItemId={Item.Id}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error navigating to map: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Unable to view on map at this time", "OK");
            }
        }

        [RelayCommand]
        private async Task ManagePhotos()
        {
            if (Item == null) return;

            try
            {
                await Shell.Current.Navigation.PushAsync(new PhotoManagementPage(Item.Id));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in ManagePhotos: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Unable to manage photos at this time", "OK");
            }
        }

        [RelayCommand]
        private async Task ManageStatus()
        {
            if (Item == null) return;

            try
            {
                await Shell.Current.Navigation.PushAsync(new StatusManagementPage(Item.Id));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in ManageStatus: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Unable to manage status at this time", "OK");
            }
        }

        // Helper method to get current user ID - replace with your actual implementation
        private async Task<int> GetCurrentUserId()
        {
            var user = await _authService.GetCurrentUserAsync();
            return user?.Id ?? 0;
        }
    }
}