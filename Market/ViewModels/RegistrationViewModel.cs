﻿// RegistrationViewModel.cs
using Market.DataAccess.Models;
using Market.Services;
using System.Windows.Input;
using Market.Helpers;
using System.Diagnostics;

namespace Market.ViewModels
{
    public class RegistrationViewModel : BindableObject
    {
        private readonly IAuthService _authService;
        private readonly IEmailService _emailService;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _confirmPassword = string.Empty;
        private bool _isBusy;

        public RegistrationViewModel(IAuthService authService, IEmailService emailService)
        {
            _authService = authService;
            _emailService = emailService;
            RegisterCommand = new Command(async () => await RegisterAsync(), () => !IsBusy);
        }

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set { _confirmPassword = value; OnPropertyChanged(); }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
                (RegisterCommand as Command)?.ChangeCanExecute();
            }
        }

        public ICommand RegisterCommand { get; }

        public async Task InitializeAsync()
        {
            // Empty for now
            await Task.CompletedTask;
        }

        private async Task RegisterAsync()
        {
            if (IsBusy) return;

            try
            {
                Debug.WriteLine("\nStarting registration process");
                IsBusy = true;

                // Validation
                if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
                {
                    Debug.WriteLine("Validation failed: Empty fields");
                    await ShowError("Please fill in all fields");
                    return;
                }

                if (!InputValidator.IsValidEmail(Email))
                {
                    Debug.WriteLine("Validation failed: Invalid email");
                    await ShowError("Please enter a valid email address");
                    return;
                }

                if (!InputValidator.IsValidPassword(Password))
                {
                    Debug.WriteLine("Validation failed: Invalid password");
                    await ShowError("Password must be at least 8 characters and contain uppercase, lowercase, and numbers");
                    return;
                }

                if (Password != ConfirmPassword)
                {
                    Debug.WriteLine("Validation failed: Passwords don't match");
                    await ShowError("Passwords do not match");
                    return;
                }

                Debug.WriteLine("All validation passed, creating user object");

                // Create user object
                var user = new User
                {
                    Email = Email,
                    PasswordHash = Password,
                    CreatedAt = DateTime.UtcNow
                };

                Debug.WriteLine("Attempting to register user");
                bool success = await _authService.RegisterUserAsync(user);

                if (success)
                {
                    Debug.WriteLine("Registration successful");

                    // Generate email verification token
                    var token = await _authService.GenerateEmailVerificationTokenAsync(user);
                    var confirmationLink = $"/confirm-email?userId={user.Id}&token={System.Net.WebUtility.UrlEncode(token)}";

                    // Send verification email
                    await _emailService.SendEmailVerificationAsync(user.Email, confirmationLink);

                    await ShowMessage("Success", "Registration successful! Please check your email for verification instructions.");
                    await Shell.Current.GoToAsync("//SignInPage");
                }
                else
                {
                    Debug.WriteLine("Registration failed: User might already exist");
                    await ShowError("Registration failed. Email might already be in use.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Registration error: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }

                // Show the actual error message
                await ShowError($"Registration failed: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ShowError(string message)
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", message, "OK");
            }
        }

        private async Task ShowMessage(string title, string message)
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert(title, message, "OK");
            }
        }
    }
}