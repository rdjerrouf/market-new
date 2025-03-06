using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Market.DataAccess.Models;
using Market.Market.DataAccess.Models;
using Market.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Market.ViewModels
{
    public partial class UserRatingsViewModel : ObservableObject
    {
        private readonly IItemService _itemService;
        private readonly IAuthService _authService;

        [ObservableProperty]
        private ObservableCollection<Rating> _ratings;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _statusMessage;

        [ObservableProperty]
        private double _averageRating;

        [ObservableProperty]
        private int _totalRatings;

        [ObservableProperty]
        private int _userId;

        [ObservableProperty]
        private string _displayName;

        [ObservableProperty]
        private bool _isCurrentUser;

        [ObservableProperty]
        private bool _hasRatings;

        public UserRatingsViewModel(IItemService itemService, IAuthService authService)
        {
            _itemService = itemService;
            _authService = authService;
            Ratings = new ObservableCollection<Rating>();
        }

        public async Task InitializeAsync(int userId)
        {
            try
            {
                IsBusy = true;
                StatusMessage = "Loading ratings...";
                Ratings.Clear();
                UserId = userId;

                // Check if this is the current user
                var currentUser = await _authService.GetCurrentUserAsync();
                IsCurrentUser = currentUser?.Id == userId;

                // Get user profile information
                var userProfile = await _authService.GetUserProfileAsync(userId);
                if (userProfile != null)
                {
                    DisplayName = userProfile.DisplayName ?? $"User {userId}";
                }

                // Get user statistics including average rating
                var stats = await _itemService.GetUserProfileStatisticsAsync(userId);
                AverageRating = stats.AverageRating;

                // Load user ratings
                var ratings = await _itemService.GetUserRatingsAsync(userId);
                foreach (var rating in ratings)
                {
                    Ratings.Add(rating);
                }

                TotalRatings = Ratings.Count;
                HasRatings = TotalRatings > 0;

                StatusMessage = string.Empty;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading ratings: {ex.Message}");
                StatusMessage = "Failed to load ratings.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task RefreshRatings()
        {
            await InitializeAsync(UserId);
        }

        [RelayCommand]
        private async Task ViewItem(int itemId)
        {
            await Shell.Current.GoToAsync($"ItemDetailPage?ItemId={itemId}");
        }

        [RelayCommand]
        private async Task MarkHelpful(Rating rating)
        {
            try
            {
                IsBusy = true;

                // This would be implemented in a real app with a method to mark a rating as helpful
                // For now, we'll just increment the count locally
                if (rating.HelpfulVotes.HasValue)
                {
                    rating.HelpfulVotes++;
                }
                else
                {
                    rating.HelpfulVotes = 1;
                }

                // Force a UI update
                var index = Ratings.IndexOf(rating);
                if (index >= 0)
                {
                    Ratings.Remove(rating);
                    Ratings.Insert(index, rating);
                }

                await Task.Delay(500); // Simulate network delay
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error marking helpful: {ex.Message}");
                StatusMessage = "Could not mark rating as helpful.";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}