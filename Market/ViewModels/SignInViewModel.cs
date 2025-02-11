using System.Windows.Input;
using System.Diagnostics;
using Market.DataAccess;
using Market.Services;
using Market.Views;

namespace Market.ViewModels
{
    public class SignInViewModel : BindableObject
    {
        private readonly IAuthService _authService;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private bool _isSigningIn;

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
            if (_isSigningIn) return;

            try
            {
                _isSigningIn = true;

                if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
                {
                    await Shell.Current.DisplayAlert("Error", "Please enter both email and password", "OK");
                    return;
                }

                Debug.WriteLine($"Attempting sign in for email: {Email}");

                var user = await _authService.SignInAsync(Email, Password);

                if (user is not null)
                {
                    Debug.WriteLine("Sign in successful, navigating to MainPage");
                    await Shell.Current.GoToAsync("//MainPage");
                }
                else
                {
                    Debug.WriteLine("Sign in failed - invalid credentials");
                    await Shell.Current.DisplayAlert("Error", "Invalid email or password", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Sign in error: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                var errorMessage = "An error occurred during sign in. ";
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    errorMessage += ex.InnerException.Message;
                }
                else
                {
                    errorMessage += ex.Message;
                }

                await Shell.Current.DisplayAlert("Error", errorMessage, "OK");
            }
            finally
            {
                _isSigningIn = false;
            }
        }

        private async Task RegisterAsync()
        {
            if (_isSigningIn) return;
            await Shell.Current.GoToAsync("//RegistrationPage");
        }
    }
}