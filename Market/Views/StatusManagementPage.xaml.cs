using Market.Services;
using Market.ViewModels;
using Market.DataAccess.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Market.Views
{
    public partial class StatusManagementPage : ContentPage
    {
        private readonly StatusManagementViewModel _viewModel;

        public StatusManagementPage(int itemId)
        {
            InitializeComponent();

            // Get services
            var statusService = Application.Current.Handler.MauiContext.Services.GetService<ItemStatusService>();
            var dbContext = Application.Current.Handler.MauiContext.Services.GetService<AppDbContext>();

            // Create and initialize the view model
            _viewModel = new StatusManagementViewModel(statusService, dbContext, Navigation);
            BindingContext = _viewModel;

            // Initialize the view model with item ID
            Loaded += async (s, e) =>
            {
                await _viewModel.InitializeAsync(itemId);
            };
        }
    }
}