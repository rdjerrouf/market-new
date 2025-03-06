using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Market.DataAccess.Models;
using Market.Market.DataAccess.Models;
using Market.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Controls;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Market.DataAccess.Data;
namespace Market.ViewModels
{
    public class PhotoManagementViewModel : ObservableObject
    {
        private readonly PhotoService _photoService;
        private readonly AppDbContext _dbContext;
        private readonly INavigation _navigation;

        private int _itemId;
        private string _itemTitle;
        private string _itemStatus;
        private bool _isRefreshing;
        private bool _isBusy;
        private ObservableCollection<ItemPhoto> _photos;

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

        public string ItemStatus
        {
            get => _itemStatus;
            set => SetProperty(ref _itemStatus, value);
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public ObservableCollection<ItemPhoto> Photos
        {
            get => _photos;
            set => SetProperty(ref _photos, value);
        }

        // Commands
        public ICommand RefreshCommand { get; }
        public ICommand AddPhotoCommand { get; }
        public ICommand PhotoTappedCommand { get; }
        public ICommand GoBackCommand { get; }

        public PhotoManagementViewModel(PhotoService photoService, AppDbContext dbContext, INavigation navigation)
        {
            _photoService = photoService;
            _dbContext = dbContext;
            _navigation = navigation;

            Photos = new ObservableCollection<ItemPhoto>();

            // Initialize commands
            RefreshCommand = new Command(async () => await RefreshPhotosAsync());
            AddPhotoCommand = new Command(async () => await AddPhotoAsync());
            PhotoTappedCommand = new Command<ItemPhoto>(async (photo) => await HandlePhotoTappedAsync(photo));
            GoBackCommand = new Command(async () => await _navigation.PopAsync());
        }

        public async Task InitializeAsync(int itemId)
        {
            ItemId = itemId;
            await LoadItemDetailsAsync();
            await RefreshPhotosAsync();
        }

        private async Task LoadItemDetailsAsync()
        {
            try
            {
                var item = await _dbContext.Items.FindAsync(ItemId);
                if (item != null)
                {
                    ItemTitle = item.Title;
                    ItemStatus = item.Status.ToString();
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to load item details: {ex.Message}", "OK");
            }
        }

        private async Task RefreshPhotosAsync()
        {
            if (ItemId <= 0)
                return;

            try
            {
                IsRefreshing = true;
                IsBusy = true;

                var photos = await _photoService.GetItemPhotosAsync(ItemId);

                Photos.Clear();
                foreach (var photo in photos)
                {
                    Photos.Add(photo);
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to load photos: {ex.Message}", "OK");
            }
            finally
            {
                IsRefreshing = false;
                IsBusy = false;
            }
        }

        private async Task AddPhotoAsync()
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    FileTypes = FilePickerFileType.Images,
                    PickerTitle = "Select a photo"
                });

                if (result != null)
                {
                    IsBusy = true;

                    var photo = await _photoService.AddPhotoAsync(ItemId, result);
                    Photos.Add(photo);

                    await Application.Current.MainPage.DisplayAlert("Success", "Photo added successfully", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to add photo: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task HandlePhotoTappedAsync(ItemPhoto photo)
        {
            if (photo == null)
                return;

            string action = await Application.Current.MainPage.DisplayActionSheet(
                "Photo Options",
                "Cancel",
                null,
                photo.IsPrimaryPhoto ? "Delete" : "Set as Primary",
                photo.IsPrimaryPhoto ? null : "Delete");

            try
            {
                IsBusy = true;

                if (action == "Set as Primary")
                {
                    await _photoService.SetPrimaryPhotoAsync(photo.Id);
                    await RefreshPhotosAsync();
                    await Application.Current.MainPage.DisplayAlert("Success", "Primary photo updated", "OK");
                }
                else if (action == "Delete")
                {
                    bool confirm = await Application.Current.MainPage.DisplayAlert(
                        "Confirm Delete",
                        "Are you sure you want to delete this photo?",
                        "Yes", "No");

                    if (confirm)
                    {
                        await _photoService.DeletePhotoAsync(photo.Id);
                        await RefreshPhotosAsync();
                        await Application.Current.MainPage.DisplayAlert("Success", "Photo deleted", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to process request: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}