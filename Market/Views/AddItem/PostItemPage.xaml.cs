using Market.ViewModels.AddItem; // Update namespace

namespace Market.Views.AddItem
{
    public partial class PostItemPage : ContentPage
    {
        public PostItemPage(AddItemViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}