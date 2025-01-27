// ViewModels/RegistrationViewModel.cs
using Market.Models;
using Market.Services;
using System.Windows.Input;

namespace Market.ViewModels
{
    /// <summary>
    /// ViewModel handling user registration logic and UI interactions
    /// </summary>
    public class RegistrationViewModel : BindableObject
    {
        private readonly IAuthService _authService;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _confirmPassword = string.Empty;

        public RegistrationViewModel(IAuthService authService)
        {
            _authService = authService;
            RegisterCommand = new Command(async () => await RegisterAsync());
        }

        // Bindable properties for the registration form
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

        // Command bound to the register button
        public ICommand RegisterCommand { get; }

        /// <summary>
        /// Handles the user registration process
        /// </summary>
        private async Task RegisterAsync()
        {
            // Validate password match
            if (Password != ConfirmPassword)
            {
                await Shell.Current.DisplayAlert("Error", "Passwords do not match", "OK");
                return;
            }

            // Create new user object
            // Note: In production, password should be hashed here
            var user = new User
            {
                Email = Email,
                PasswordHash = Password, // TODO: Implement proper password hashing
            };

            // Attempt registration
            bool success = await _authService.RegisterUserAsync(user);
            if (success)
            {
                // Navigate to main page on success
                await Shell.Current.GoToAsync("//MainPage");
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "Registration failed", "OK");
            }
        }
    }
}