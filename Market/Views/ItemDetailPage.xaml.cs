using Market.ViewModels;

namespace Market.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage(ItemDetailViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}