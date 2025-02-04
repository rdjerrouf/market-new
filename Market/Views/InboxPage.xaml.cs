using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Market.ViewModels;
using System.Diagnostics;

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

        // Add this method
        /*   protected override void OnAppearing()
           {
               Debug.WriteLine("InboxPage: OnAppearing started");
               base.OnAppearing();
               if (BindingContext is InboxViewModel viewModel)
               {
                   Debug.WriteLine("InboxPage: BindingContext is InboxViewModel, executing LoadMessagesCommand");
                   viewModel.LoadMessagesCommand.Execute(null);
               }
               else
               {
                   Debug.WriteLine("InboxPage: BindingContext is not InboxViewModel");
               }
               Debug.WriteLine("InboxPage: OnAppearing completed"); need to go back to this function, now we're testing */
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