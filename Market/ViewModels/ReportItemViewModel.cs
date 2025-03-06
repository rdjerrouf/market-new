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
    [QueryProperty(nameof(ItemId), "ItemId")]
    public partial class ReportItemViewModel : ObservableObject
    {
        private readonly SecurityService _securityService;
        private readonly IAuthService _authService;

        public ReportItemViewModel(SecurityService securityService, IAuthService authService)
        {
            _securityService = securityService;
            _authService = authService;

            ReportReasons = new ObservableCollection<string>
            {
                "Prohibited content",
                "Incorrect category",
                "Scam or fraud",
                "Offensive content",
                "Duplicate listing",
                "Copyright violation",
                "Other"
            };
        }

        [ObservableProperty]
        private int itemId;

        [ObservableProperty]
        private string title = "Report Item";

        [ObservableProperty]
        private string selectedReason = string.Empty;

        [ObservableProperty]
        private string additionalComments = string.Empty;

        [ObservableProperty]
        private ObservableCollection<string> reportReasons;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private bool hasAlreadyReported;

        private async Task CheckIfAlreadyReportedAsync()
        {
            if (ItemId <= 0)
                return;

            try
            {
                IsBusy = true;
                // Get current user ID
                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser == null)
                {
                    await Shell.Current.DisplayAlert("Error", "You must be logged in to report items", "OK");
                    await Shell.Current.GoToAsync("..");
                    return;
                }

                HasAlreadyReported = await _securityService.HasUserReportedItemAsync(currentUser.Id, ItemId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking report status: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task SubmitReport()
        {
            if (ItemId <= 0 || string.IsNullOrWhiteSpace(SelectedReason))
            {
                await Shell.Current.DisplayAlert("Error", "Please select a reason for reporting", "OK");
                return;
            }

            try
            {
                IsBusy = true;

                // Get current user ID
                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser == null)
                {
                    await Shell.Current.DisplayAlert("Error", "You must be logged in to report items", "OK");
                    return;
                }

                await _securityService.ReportItemAsync(
                    ItemId,
                    currentUser.Id,
                    SelectedReason,
                    AdditionalComments);

                await Shell.Current.DisplayAlert(
                    "Thank You",
                    "Your report has been submitted and will be reviewed by our team.",
                    "OK");

                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error submitting report: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to submit report. Please try again.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task Cancel()
        {
            await Shell.Current.GoToAsync("..");
        }

        public async Task InitializeAsync()
        {
            await CheckIfAlreadyReportedAsync();
        }
    }
}