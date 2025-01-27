using System.Windows.Input;
using Market.Models;
using Market.Services;

namespace Market.ViewModels
{
    public class SignInViewModel : BindableObject
    {
        private readonly IAuthService _authService;
        private string _email = string.Empty;
        private string _password = string.Empty;

        public SignInViewModel(IAuthService authService)
        {
            _authService = authService;
            SignInCommand = new Command(async () => await SignInAsync());
            RegisterCommand = new Command(async () => await RegisterAsync());
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

        public ICommand SignInCommand { get; }
        public ICommand RegisterCommand { get; }

        private async Task SignInAsync()
        {
            // Hash the password before comparing
            var hashedPassword = PasswordHasher.HashPassword(Password);
            var user = await _authService.SignInAsync(Email, hashedPassword);

            if (user is not null)
            {
                await Shell.Current.GoToAsync("//MainPage");
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "Invalid credentials", "OK");
            }
        }

        /// <summary>
        /// Navigates to the registration page
        /// </summary>
        private async Task RegisterAsync()
        {
            await Shell.Current.GoToAsync("//RegistrationPage");
        }
    }
}