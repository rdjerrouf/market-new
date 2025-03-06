using Market.Services;
using Market.ViewModels;
using Market.DataAccess.Data;
using Market.DataAccess.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Market.Views
{
    public partial class PhotoManagementPage : ContentPage
    {
        private readonly PhotoManagementViewModel _viewModel;

        public PhotoManagementPage(int itemId)
        {
            InitializeComponent();

            // Get services
            var photoService = Application.Current.Handler.MauiContext.Services.GetService<PhotoService>();
            var dbContext = Application.Current.Handler.MauiContext.Services.GetService<AppDbContext>();

            // Create and initialize the view model
            _viewModel = new PhotoManagementViewModel(photoService, dbContext, Navigation);
            BindingContext = _viewModel;

            // Initialize the view model with item ID
            Loaded += async (s, e) =>
            {
                await _viewModel.InitializeAsync(itemId);
            };
        }
    }
}