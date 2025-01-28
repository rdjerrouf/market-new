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

        // Fields to store user input
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _confirmPassword = string.Empty;
        private bool _isBusy;

        public RegistrationViewModel(IAuthService authService)
        {
            _authService = authService;
            // Initialize the register command with canExecute parameter
            RegisterCommand = new Command(async () => await RegisterAsync(), () => !IsBusy);
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

        // Handles the registration process
        private async Task RegisterAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                // Basic validation
                if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
                {
                    await Shell.Current.DisplayAlert("Error", "Please fill in all fields", "OK");
                    return;
                }

                // Check if passwords match
                if (Password != ConfirmPassword)
                {
                    await Shell.Current.DisplayAlert("Error", "Passwords do not match", "OK");
                    return;
                }

                // Create new user object
                var user = new User
                {
                    Email = Email,
                    PasswordHash = Password // Note: Should implement proper hashing
                };

                // Attempt registration
                bool success = await _authService.RegisterUserAsync(user);
                if (success)
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await Shell.Current.GoToAsync("//MainPage");
                    });
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Registration failed", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Registration error: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}