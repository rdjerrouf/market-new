using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Market.DataAccess.Models;
using Market.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Market.ViewModels
{
    public partial class BlockedUsersViewModel : ObservableObject
    {
        private readonly SecurityService _securityService;
        private readonly IAuthService _authService;

        public BlockedUsersViewModel(SecurityService securityService, IAuthService authService)
        {
            _securityService = securityService;
            _authService = authService;
            BlockedUsers = new ObservableCollection<User>();
        }

        private ObservableCollection<User> _blockedUsers;
        public ObservableCollection<User> BlockedUsers
        {
            get => _blockedUsers;
            set => SetProperty(ref _blockedUsers, value);
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        [RelayCommand]
        private async Task Refresh()
        {
            IsRefreshing = true;
            await LoadBlockedUsersAsync();
            IsRefreshing = false;
        }

        public async Task LoadBlockedUsersAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;

                // Get current user ID
                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser == null)
                {
                    await Shell.Current.DisplayAlert("Error", "You must be logged in to view blocked users", "OK");
                    return;
                }

                var users = await _securityService.GetBlockedUsersAsync(currentUser.Id);

                BlockedUsers.Clear();
                foreach (var user in users)
                {
                    BlockedUsers.Add(user);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading blocked users: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to load blocked users", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task UnblockUser(User user)
        {
            if (user == null)
                return;

            bool confirm = await Shell.Current.DisplayAlert(
                "Confirm Unblock",
                $"Are you sure you want to unblock {user.DisplayName ?? user.Email}?",
                "Yes", "No");

            if (!confirm)
                return;

            try
            {
                IsBusy = true;

                // Get current user ID
                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser == null)
                {
                    await Shell.Current.DisplayAlert("Error", "You must be logged in to unblock users", "OK");
                    return;
                }

                bool success = await _securityService.UnblockUserAsync(currentUser.Id, user.Id);

                if (success)
                {
                    BlockedUsers.Remove(user);
                    await Shell.Current.DisplayAlert("Success", "User has been unblocked", "OK");
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