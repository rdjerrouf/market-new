// ViewModels/PasswordResetViewModel.cs
using System.Diagnostics;
using System.Windows.Input;
using Market.DataAccess.Models;
using Market.Helpers;
using Market.Services;

namespace Market.ViewModels
{
    /// <summary>
    /// ViewModel for handling password reset functionality
    /// </summary>
    public class PasswordResetViewModel : BindableObject
    {
        private readonly IAuthService _authService;

        private string _email = string.Empty;
        private string _newPassword = string.Empty;
        private string _confirmPassword = string.Empty;
        private string _errorMessage = string.Empty;

        /// <summary>
        /// Email of the user requesting password reset
        /// </summary>
        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged();
                ValidateEmail();
            }
        }

        /// <summary>
        /// New password for the user
        /// </summary>
        public string NewPassword
        {
            get => _newPassword;
            set
            {
                _newPassword = value;
                OnPropertyChanged();
                ValidatePassword();
            }
        }

        /// <summary>
        /// Confirmation of the new password
        /// </summary>
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                _confirmPassword = value;
                OnPropertyChanged();
                ValidatePassword();
            }
        }

        /// <summary>
        /// Error message to display to the user
        /// </summary>
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Command to initiate password reset
        /// </summary>
        public ICommand ResetPasswordCommand { get; }

        /// <summary>
        /// Constructor for PasswordResetViewModel
        /// </summary>
        /// <param name="authService">Authentication service for password reset</param>
        public PasswordResetViewModel(IAuthService authService)
        {
            _authService = authService;
            ResetPasswordCommand = new Command(async () => await ResetPasswordAsync(), CanResetPassword);
        }

        /// <summary>
        /// Validates email format
        /// </summary>
        private bool _isEmailValid;
        private void ValidateEmail()
        {
            _isEmailValid = InputValidator.IsValidEmail(Email);
            UpdateErrorMessage();
            ((Command)ResetPasswordCommand).ChangeCanExecute();
        }

        /// <summary>
        /// Validates password strength and confirmation
        /// </summary>
        private bool _isPasswordValid;
        private void ValidatePassword()
        {
            _isPasswordValid = InputValidator.IsValidPassword(NewPassword) &&
                               NewPassword == ConfirmPassword;
            UpdateErrorMessage();
            ((Command)ResetPasswordCommand).ChangeCanExecute();
        }

        /// <summary>
        /// Updates error message based on validation results
        /// </summary>
        private void UpdateErrorMessage()
        {
            ErrorMessage = !_isEmailValid ? "Invalid email format" :
                           !_isPasswordValid ? "Invalid password or passwords do not match" :
                           string.Empty;
        }

        /// <summary>
        /// Determines if password reset can be attempted
        /// </summary>
        private bool CanResetPassword()
        {
            return _isEmailValid && _isPasswordValid;
        }

        /// <summary>
        /// Attempts to reset the user's password
        /// </summary>
        private async Task ResetPasswordAsync()
        {
            try
            {
                // Clear previous error
                ErrorMessage = string.Empty;

                // Verify email and password
                if (!_isEmailValid || !_isPasswordValid)
                {
                    await Shell.Current.DisplayAlert("Error", "Please correct input errors", "OK");
                    return;
                }

                // Attempt password reset
                bool resetSuccessful = await _authService.ChangePasswordAsync(Email, NewPassword, NewPassword);

                if (resetSuccessful)
                {
                    await Shell.Current.DisplayAlert("Success", "Password reset successful", "OK");
                    await Shell.Current.GoToAsync("//SignInPage");
                }
                else
                {
                    ErrorMessage = "Password reset failed";
                    await Shell.Current.DisplayAlert("Error", "Unable to reset password", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Password reset error: {ex.Message}");
                ErrorMessage = "An error occurred during password reset";
                await Shell.Current.DisplayAlert("Error", $"Reset error: {ex.Message}", "OK");
            }
        }
    }
}