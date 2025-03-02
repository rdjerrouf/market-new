using Market.Services;
using Market.ViewModels;

namespace Market.Views
{
    public partial class MessageDetailPage : ContentPage
    {
        private readonly MessageDetailViewModel _viewModel;

        public MessageDetailPage(MessageDetailViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            _viewModel = viewModel;
        }

        // In MessageDetailPage.xaml.cs
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                // Get the message ID from query parameter using your extension method
                var messageIdString = await Shell.Current.GetQueryParameterAsync("MessageId");

                if (!string.IsNullOrEmpty(messageIdString) && int.TryParse(messageIdString, out int messageId))
                {
                    await _viewModel.InitializeAsync(messageId);
                }
                else
                {
                    await DisplayAlert("Error", "Invalid message ID", "OK");
                    await Shell.Current.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
                await Shell.Current.GoToAsync("..");
            }
        }
    }
}