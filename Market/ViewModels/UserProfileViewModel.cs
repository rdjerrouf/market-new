// ViewModels/UserProfileViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Market.DataAccess.Models;
using Market.Market.DataAccess.Models.Dtos;
using Market.Services;
using System.Diagnostics;

namespace Market.ViewModels
{
    [QueryProperty(nameof(UserId), "UserId")]
    public partial class UserProfileViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly IItemService _itemService;
        private readonly SecurityService _securityService;

        [ObservableProperty]
        private int userId;

        [ObservableProperty]
        private UserProfileDto profile;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private string statusMessage;

        [ObservableProperty]
        private bool isCurrentUser;

        [ObservableProperty]
        private int postedItemsCount;

        [ObservableProperty]
        private double averageRating;

        [ObservableProperty]
        private bool isUserBlocked;

        public UserProfileViewModel(IAuthService authService, IItemService itemService, SecurityService securityService)
        {
            _authService = authService;
            _itemService = itemService;
            _securityService = securityService;
            Profile = new UserProfileDto();
        }

        public async Task InitializeAsync()
        {
            if (UserId <= 0)
                return;

            try
            {
                IsBusy = true;

                // Get current user to check if this is the current user's profile
                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser != null)
                {
                    IsCurrentUser = UserId == currentUser.Id;

                    // Check if this user is blocked
                    IsUserBlocked = await _securityService.IsUserBlockedAsync(currentUser.Id, UserId);
                }

                // Load profile
                await LoadProfileAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing profile: {ex.Message}");
                StatusMessage = "Failed to load profile";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadProfileAsync()
        {
            try
            {
                var userProfile = await _authService.GetUserProfileAsync(UserId);
                if (userProfile != null)
                {
                    Profile = userProfile;

                    // Load statistics
                    var stats = await _itemService.GetUserProfileStatisticsAsync(UserId);
                    PostedItemsCount = stats.PostedItemsCount;
                    AverageRating = stats.AverageRating;
                }
                else
                {
                    StatusMessage = "Could not load profile";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading profile: {ex.Message}");
                StatusMessage = "Failed to load profile data";
            }
        }

        [RelayCommand]
        private async Task ViewUserItems()
        {
            await Shell.Current.GoToAsync($"//UserItemsPage?UserId={UserId}");
        }

        [RelayCommand]
        private async Task ViewUserRatings()
        {
            await Shell.Current.GoToAsync($"UserRatingsPage?UserId={UserId}");
        }

        [RelayCommand]
        private async Task BlockUser()
        {
            if (IsCurrentUser)
                return; // Can't block yourself

            bool confirm = await Shell.Current.DisplayAlert(
            "Block User",
            $"Are you sure you want to block {Profile.DisplayName ?? Profile.Email}? They won't be able to see your listings or contact you.",
            "Block", "Cancel");

            if (!confirm)
                return;

            try
            {
                IsBusy = true;

                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser == null)
                    return;

                await _securityService.BlockUserAsync(
                    currentUser.Id,
                    UserId,
                    "Blocked by user");
                await Shell.Current.DisplayAlert(
                "User Blocked",
                $"{Profile.DisplayName ?? Profile.Email} has been blocked",
                    "OK");

                IsUserBlocked = true;

                // Optionally navigate back
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error blocking user: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to block user", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task UnblockUser()
        {
            if (!IsUserBlocked)
                return;

            bool confirm = await Shell.Current.DisplayAlert(
            "Unblock User",
            $"Are you sure you want to unblock {Profile.DisplayName ?? Profile.Email}?",
                "Unblock", "Cancel");

            if (!confirm)
                return;

            try
            {
                IsBusy = true;

                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser == null)
                    return;

                bool success = await _securityService.UnblockUserAsync(
                    currentUser.Id,
                    UserId);

                if (success)
                {
                    await Shell.Current.DisplayAlert(
                    "User Unblocked",
                    $"{Profile.DisplayName ?? Profile.Email} has been unblocked",
                        "OK");

                    IsUserBlocked = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error unblocking user: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to unblock user", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}