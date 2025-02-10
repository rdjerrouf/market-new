using Market.ViewModels.AddItem;
namespace Market.Views.AddItem;

public partial class AddItemPage : ContentPage
{
	public AddItemPage()
	{
		InitializeComponent();
        BindingContext = new AddItemViewModel();
    }
}