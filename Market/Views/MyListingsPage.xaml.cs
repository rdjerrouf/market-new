using Market.ViewModels;
using Market.DataAccess.Models;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace Market.Views
{
    public partial class MyListingsPage : ContentPage
    {
        public MyListingsPage(MyListingsViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is MyListingsViewModel viewModel)
            {
                viewModel.LoadMyListingsCommand.Execute(null);
            }
        }
       
    }
}