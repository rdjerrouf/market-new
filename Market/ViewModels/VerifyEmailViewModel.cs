using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Market.Services;
using System.Diagnostics;
using System.Windows.Input;

namespace Market.ViewModels
{
    public partial class VerifyEmailViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly IEmailService _emailService;
        private int _userId;
        private string _email;
        private CancellationTokenSource? _countdownCts;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private Color _statusMessageColor = Colors.Black;

        [ObservableProperty]
        private int _resendCountdown;

        [ObservableProperty]
        private bool _isCountdownVisible;

        [ObservableProperty]
        private bool _canResend = true;

        public bool HasStatusMessage => !string.IsNullOrEmpty(StatusMessage);

        public VerifyEmailViewModel(IAuthService authService, IEmailService emailService)
        {
            _authService = authService;
            _emailService = emailService;
        }

        public async Task InitializeAsync(int userId, string email)
        {
            _userId = userId;
            _email = email;

            bool isVerified = await _authService.IsEmailVerifiedAsync(userId);
            if (isVerified)
            {
                StatusMessage = "Your email is already verified!";
                StatusMessageColor = Colors.Green;
            }
        }

        [RelayCommand]
        private async Task ResendVerification()
        {
            if (IsBusy || !CanResend) return;

            try
            {
                IsBusy = true;
                StatusMessage = string.Empty;

                // Generate a new verification token
                var user = await _authService.GetCurrentUserAsync();
                if (user == null)
                {
                    StatusMessage = "You need to be logged in to verify your email.";
                    StatusMessageColor = Colors.Red;
                    return;
                }

                string token = await _authService.GenerateEmailVerificationTokenAsync(user);

                // Create verification link
                // For MAUI apps, you'd typically use a deep link or a web verification page
                string verificationLink = $"https://yourapp.com/verify?userId={user.Id}&token={token}";

                // Send verification email
                bool emailSent = await _emailService.SendEmailVerificationAsync(_email, verificationLink);

                if (emailSent)
                {
                    StatusMessage = "Verification email sent! Please check your inbox.";
                    StatusMessageColor = Colors.Green;

                    // Start countdown for resend
                    StartResendCountdown();
                }
                else
                {
                    StatusMessage = "Failed to send verification email. Please try again.";
                    StatusMessageColor = Colors.Red;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error sending verification email: {ex.Message}");
                StatusMessage = "An error occurred. Please try again later.";
                StatusMessageColor = Colors.Red;
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task CheckVerification()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                StatusMessage = string.Empty;

                bool isVerified = await _authService.IsEmailVerifiedAsync(_userId);

                if (isVerified)
                {
                    StatusMessage = "Email verified successfully!";
                    StatusMessageColor = Colors.Green;

                    // Wait a moment for user to see the success message
                    await Task.Delay(1500);

                    // Navigate back to profile
                    await BackToProfile();
                }
                else
                {
                    StatusMessage = "Email not verified yet. Please check your inbox and click the verification link.";
                    StatusMessageColor = Colors.Orange;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking verification: {ex.Message}");
                StatusMessage = "An error occurred. Please try again later.";
                StatusMessageColor = Colors.Red;
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task BackToProfile()
        {
            await Shell.Current.GoToAsync(".."); // Navigate back
        }

        private void StartResendCountdown()
        {
            // Cancel any existing countdown
            _countdownCts?.Cancel();
            _countdownCts = new CancellationTokenSource();

            ResendCountdown = 60; // 60 second cooldown
            CanResend = false;
            IsCountdownVisible = true;

            // Start countdown timer
            Task.Run(async () =>
            {
                while (ResendCountdown > 0)
                {
                    try
                    {
                        await Task.Delay(1000, _countdownCts.Token);
                        MainThread.BeginInvokeOnMainThread(() => ResendCountdown--);
                    }
                    catch (TaskCanceledException)
                    {
                        return;
                    }
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    CanResend = true;
                    IsCountdownVisible = false;
                });
            }, _countdownCts.Token);
        }

        public void Cleanup()
        {
            _countdownCts?.Cancel();
            _countdownCts?.Dispose();
            _countdownCts = null;
        }
    }
}