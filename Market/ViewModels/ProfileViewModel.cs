using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Market.DataAccess.Models;
using Market.Market.DataAccess.Models.Dtos;
using Market.Services;
using Market.Views;
using System.Diagnostics;

namespace Market.ViewModels
{
    public partial class ProfileViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly IItemService _itemService;
        private readonly SecurityService _securityService;

        [ObservableProperty]
        private UserProfileDto _profile;


        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _statusMessage;

        [ObservableProperty]
        private bool _isEmailVerificationNeeded;

        [ObservableProperty]
        private string _displayName;

        [ObservableProperty]
        private string _bio;

        [ObservableProperty]
        private string _phoneNumber;

        [ObservableProperty]
        private string _city;

        [ObservableProperty]
        private string _province;

        [ObservableProperty]
        private bool _showEmail;

        [ObservableProperty]
        private bool _showPhoneNumber;

        [ObservableProperty]
        private int _postedItemsCount;

        [ObservableProperty]
        private int _favoriteItemsCount;

        [ObservableProperty]
        private string _profilePicture;

        [ObservableProperty]
        private bool _isEditing;

        [ObservableProperty]
        private bool _isLoggedIn;

        // Added property for average rating
        [ObservableProperty]
        private double _averageRating;

        public ProfileViewModel(IAuthService authService, IItemService itemService, SecurityService securityService)
        {
            _authService = authService;
            _itemService = itemService;
            _securityService = securityService;
            Profile = new UserProfileDto();
        }
        public async Task InitializeAsync()
        {
            try
            {
                IsBusy = true;

                var currentUser = await _authService.GetCurrentUserAsync();
                IsLoggedIn = currentUser != null;

                if (IsLoggedIn && currentUser != null)
                {
                    // Load profile data
                    await LoadProfileAsync(currentUser.Id);

                    // Check if email verification is needed
                    IsEmailVerificationNeeded = !(await _authService.IsEmailVerifiedAsync(currentUser.Id));
                }
                else
                {
                    StatusMessage = "You must be logged in to view your profile";
                }
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

        private async Task LoadProfileAsync(int userId)
        {
            try
            {
                var profile = await _authService.GetUserProfileAsync(userId);
                if (profile != null)
                {
                    Profile = profile;

                    // Initialize editable fields
                    DisplayName = profile.DisplayName ?? string.Empty;
                    Bio = profile.Bio ?? string.Empty;
                    PhoneNumber = profile.PhoneNumber ?? string.Empty;
                    City = profile.City ?? string.Empty;
                    Province = profile.Province ?? string.Empty;
                    ProfilePicture = profile.ProfilePicture ?? string.Empty;

                    // Load statistics
                    var stats = await _itemService.GetUserProfileStatisticsAsync(userId);
                    PostedItemsCount = stats.PostedItemsCount;
                    FavoriteItemsCount = stats.FavoriteItemsCount;
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
        private void EditProfile()
        {
            IsEditing = true;
        }

        [RelayCommand]
        private async Task SignIn()
        {
            await Shell.Current.GoToAsync("///SignInPage");
        }

        [RelayCommand]
        private async Task SaveProfile()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser == null)
                {
                    StatusMessage = "You must be logged in to update your profile";
                    return;
                }

                // Update profile
                bool profileUpdated = await _authService.UpdateUserProfileAsync(
                    currentUser.Id,
                    DisplayName,
                    ProfilePicture,
                    Bio);

                // Update contact info
                bool contactUpdated = await _authService.UpdateUserContactInfoAsync(
                    currentUser.Id,
                    PhoneNumber,
                    City,
                    Province);

                // Update privacy settings
                bool privacyUpdated = await _authService.UpdateUserPrivacyAsync(
                    currentUser.Id,
                    ShowEmail,
                    ShowPhoneNumber);

                if (profileUpdated && contactUpdated && privacyUpdated)
                {
                    StatusMessage = "Profile updated successfully";
                    IsEditing = false;

                    // Reload profile data
                    await LoadProfileAsync(currentUser.Id);
                }
                else
                {
                    StatusMessage = "Failed to update profile";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving profile: {ex.Message}");
                StatusMessage = "An error occurred while saving profile";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void CancelEdit()
        {
            IsEditing = false;

            // Reset editable fields to original values
            DisplayName = Profile.DisplayName;
            Bio = Profile.Bio;
            PhoneNumber = Profile.PhoneNumber;
            City = Profile.City;
            Province = Profile.Province;
        }

        [RelayCommand]
        private async Task VerifyEmail()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                var user = await _authService.GetCurrentUserAsync();

                if (user != null)
                {
                    // Send verification email
                    bool emailSent = await _authService.SendEmailVerificationTokenAsync(user.Id);

                    if (emailSent)
                    {
                        // Navigate to verification page
                        var navigationParameter = new Dictionary<string, object>
                                        {
                                            { "userId", user.Id.ToString() },
                                            { "email", user.Email }
                                        };

                        await Shell.Current.GoToAsync("VerifyEmailPage", navigationParameter);
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("Error", "Failed to send verification email", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error starting email verification: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "An error occurred", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task ViewPostedItems()
        {
            await Shell.Current.GoToAsync("//MyListingsPage");
        }

        [RelayCommand]
        private async Task ViewFavoriteItems()
        {
            // Navigate to favorites page
            await Shell.Current.GoToAsync("//FavoritesPage");
        }

        // Add the ViewRatings command
        [RelayCommand]
        private async Task ViewRatings()
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser != null)
            {
                await Shell.Current.GoToAsync($"{nameof(UserRatingsPage)}?UserId={currentUser.Id}");
            }
        }

        [RelayCommand]
        private async Task ChangePassword()
        {
            // Navigate to password change page
            await Shell.Current.GoToAsync("PasswordChangePage");
        }

        [RelayCommand]
        private async Task SignOut()
        {
            try
            {
                // Clear the user ID from secure storage
                await SecureStorage.SetAsync("userId", string.Empty);

                // Navigate to sign in page
                await Shell.Current.GoToAsync("///SignInPage");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error signing out: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to sign out", "OK");
            }
        }

        [RelayCommand]
        private async Task UploadProfilePicture()
        {
            try
            {
                var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Select Profile Picture"
                });

                if (result != null)
                {
                    // Save the photo to local storage
                    var stream = await result.OpenReadAsync();
                    var fileName = $"profile_{Guid.NewGuid()}{Path.GetExtension(result.FileName)}";
                    var localPath = Path.Combine(FileSystem.CacheDirectory, fileName);

                    using (var fileStream = File.Create(localPath))
                    {
                        await stream.CopyToAsync(fileStream);
                    }

                    ProfilePicture = localPath;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error uploading profile picture: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to upload profile picture", "OK");
            }
        }

        // Add this to your ProfileViewModel.cs
        [RelayCommand]
        private async Task ManageBlockedUsers()
        {
            await Shell.Current.GoToAsync($"{nameof(BlockedUsersPage)}");
        }



    }
}