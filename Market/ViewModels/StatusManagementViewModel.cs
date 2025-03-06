using System;
using Market.DataAccess.Data;
using System.Windows.Input;
using Market.Market.DataAccess.Models;
using Market.Services;
using Market.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Controls;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Market.ViewModels
{
    public class StatusManagementViewModel : ObservableObject
    {
        private readonly ItemStatusService _statusService;
        private readonly AppDbContext _dbContext;
        private readonly INavigation _navigation;

        private int _itemId;
        private string _itemTitle;
        private string _statusText;
        private string _statusColor;
        private string _photoUrl;
        private bool _hasPhoto;
        private bool _isBusy;
        private ItemStatus _currentStatus;

        public int ItemId
        {
            get => _itemId;
            set => SetProperty(ref _itemId, value);
        }

        public string ItemTitle
        {
            get => _itemTitle;
            set => SetProperty(ref _itemTitle, value);
        }

        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        public string StatusColor
        {
            get => _statusColor;
            set => SetProperty(ref _statusColor, value);
        }

        public string PhotoUrl
        {
            get => _photoUrl;
            set => SetProperty(ref _photoUrl, value);
        }

        public bool HasPhoto
        {
            get => _hasPhoto;
            set => SetProperty(ref _hasPhoto, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        // Status change enablement properties
        public bool CanMarkAsActive => _currentStatus != ItemStatus.Active;
        public bool CanMarkAsSold => _currentStatus != ItemStatus.Sold;
        public bool CanMarkAsRented => _currentStatus != ItemStatus.Rented;
        public bool CanMarkAsUnavailable => _currentStatus != ItemStatus.Unavailable;

        // Commands
        public ICommand MarkAsActiveCommand { get; }
        public ICommand MarkAsSoldCommand { get; }
        public ICommand MarkAsRentedCommand { get; }
        public ICommand MarkAsUnavailableCommand { get; }
        public ICommand ManagePhotosCommand { get; }
        public ICommand GoBackCommand { get; }

        public StatusManagementViewModel(ItemStatusService statusService, AppDbContext dbContext, INavigation navigation)
        {
            _statusService = statusService;
            _dbContext = dbContext;
            _navigation = navigation;

            // Initialize commands
            MarkAsActiveCommand = new Command(async () => await ExecuteMarkAsActiveCommand(), () => CanMarkAsActive);
            MarkAsSoldCommand = new Command(async () => await ExecuteMarkAsSoldCommand(), () => CanMarkAsSold);
            MarkAsRentedCommand = new Command(async () => await ExecuteMarkAsRentedCommand(), () => CanMarkAsRented);
            MarkAsUnavailableCommand = new Command(async () => await ExecuteMarkAsUnavailableCommand(), () => CanMarkAsUnavailable);
            ManagePhotosCommand = new Command(async () => await ExecuteManagePhotosCommand());
            GoBackCommand = new Command(async () => await _navigation.PopAsync());
        }

        public async Task InitializeAsync(int itemId)
        {
            ItemId = itemId;
            await LoadItemDataAsync();
        }

        private async Task LoadItemDataAsync()
        {
            if (ItemId <= 0)
                return;

            try
            {
                IsBusy = true;

                var item = await _dbContext.Items
                    .Include(i => i.Photos)
                    .FirstOrDefaultAsync(i => i.Id == ItemId);

                if (item != null)
                {
                    ItemTitle = item.Title;
                    _currentStatus = item.Status;
                    UpdateStatusUI(item.Status);

                    // Get the primary photo
                    var primaryPhoto = item.Photos.FirstOrDefault(p => p.IsPrimaryPhoto);
                    if (primaryPhoto != null)
                    {
                        PhotoUrl = primaryPhoto.PhotoUrl;
                        HasPhoto = true;
                    }
                    else if (!string.IsNullOrEmpty(item.PhotoUrl))
                    {
                        PhotoUrl = item.PhotoUrl;
                        HasPhoto = true;
                    }
                    else
                    {
                        HasPhoto = false;
                    }

                    // Update command can execute states
                    ((Command)MarkAsActiveCommand).ChangeCanExecute();
                    ((Command)MarkAsSoldCommand).ChangeCanExecute();
                    ((Command)MarkAsRentedCommand).ChangeCanExecute();
                    ((Command)MarkAsUnavailableCommand).ChangeCanExecute();
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to load item: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void UpdateStatusUI(ItemStatus status)
        {
            StatusText = status.ToString();

            StatusColor = status switch
            {
                ItemStatus.Active => "#28a745",      // Green
                ItemStatus.Sold => "#dc3545",        // Red
                ItemStatus.Rented => "#fd7e14",      // Orange
                ItemStatus.Unavailable => "#6c757d", // Gray
                _ => "#28a745"                       // Default Green
            };
        }

        private async Task ExecuteMarkAsActiveCommand()
        {
            await ChangeItemStatusAsync(ItemStatus.Active);
        }

        private async Task ExecuteMarkAsSoldCommand()
        {
            await ChangeItemStatusAsync(ItemStatus.Sold);
        }

        private async Task ExecuteMarkAsRentedCommand()
        {
            await ChangeItemStatusAsync(ItemStatus.Rented);
        }

        private async Task ExecuteMarkAsUnavailableCommand()
        {
            await ChangeItemStatusAsync(ItemStatus.Unavailable);
        }

        private async Task ChangeItemStatusAsync(ItemStatus newStatus)
        {
            try
            {
                IsBusy = true;

                bool success = false;

                switch (newStatus)
                {
                    case ItemStatus.Active:
                        success = await _statusService.MarkAsActiveAsync(ItemId);
                        break;
                    case ItemStatus.Sold:
                        success = await _statusService.MarkAsSoldAsync(ItemId);
                        break;
                    case ItemStatus.Rented:
                        success = await _statusService.MarkAsRentedAsync(ItemId);
                        break;
                    case ItemStatus.Unavailable:
                        success = await _statusService.MarkAsUnavailableAsync(ItemId);
                        break;
                }

                if (success)
                {
                    _currentStatus = newStatus;
                    UpdateStatusUI(newStatus);

                    // Update commands
                    ((Command)MarkAsActiveCommand).ChangeCanExecute();
                    ((Command)MarkAsSoldCommand).ChangeCanExecute();
                    ((Command)MarkAsRentedCommand).ChangeCanExecute();
                    ((Command)MarkAsUnavailableCommand).ChangeCanExecute();

                    await Application.Current.MainPage.DisplayAlert(
                        "Success",
                        $"Item marked as {newStatus}",
                        "OK");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        "Failed to update item status",
                        "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"Failed to update status: {ex.Message}",
                    "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ExecuteManagePhotosCommand()
        {
            // Navigate to photo management page
            var photoManagementPage = new PhotoManagementPage(ItemId);
            await _navigation.PushAsync(photoManagementPage);
        }
    }
}