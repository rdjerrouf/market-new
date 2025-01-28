// ViewModels/MainViewModel.cs

using System.Windows.Input;

namespace Market.ViewModels
{
    // Handles the main page logic and interactions
    public class MainViewModel : BindableObject
    {
        // Fields to store page state
        private string _welcomeMessage = "Welcome to Dlala!";
        private bool _isLoading = false;

        public MainViewModel()
        {
            // Initialize commands
            SignOutCommand = new Command(async () => await SignOutAsync());
        }

        // Bindable property for welcome message
        public string WelcomeMessage
        {
            get => _welcomeMessage;
            set { _welcomeMessage = value; OnPropertyChanged(); }
        }

        // Bindable property for loading state
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        // Command for sign out functionality
        public ICommand SignOutCommand { get; }

        // Handle sign out process
        private async Task SignOutAsync()
        {
            IsLoading = true;
            try
            {
                // Navigate back to sign in page
                await Shell.Current.GoToAsync("//SignInPage");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}