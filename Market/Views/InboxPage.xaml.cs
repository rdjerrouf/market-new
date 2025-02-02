using Microsoft.Maui.Controls;
using Market.ViewModels;

namespace Market.Views
{
    public partial class InboxPage : ContentPage
    {
        public InboxPage(InboxViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}