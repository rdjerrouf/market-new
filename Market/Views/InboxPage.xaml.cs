using System.Diagnostics;
using Market.ViewModels;
using Market.Converters;

namespace Market.Views
{
    public partial class InboxPage : ContentPage
    {
        public InboxPage(InboxViewModel viewModel)
        {
            Debug.WriteLine("InboxPage: Constructor started");
            InitializeComponent();
            BindingContext = viewModel;
            Debug.WriteLine("InboxPage: BindingContext set to InboxViewModel");
            Debug.WriteLine("InboxPage: InitializeComponent completed");
            Debug.WriteLine("InboxPage: Constructor completed");
        }

        protected override async void OnAppearing()
        {
            Debug.WriteLine("InboxPage: OnAppearing started");
            base.OnAppearing();
            if (BindingContext is InboxViewModel viewModel)
            {
                Debug.WriteLine("InboxPage: BindingContext is InboxViewModel, executing LoadMessagesCommand");
                await viewModel.LoadMessagesCommand.ExecuteAsync(null);
            }
            else
            {
                Debug.WriteLine("InboxPage: BindingContext is not InboxViewModel");
            }
            Debug.WriteLine("InboxPage: OnAppearing completed");
        }
    }
}