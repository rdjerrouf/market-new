using System.Diagnostics;
using Market.ViewModels;

namespace Market.Views;

public partial class PostItemPage : ContentPage
{
    private readonly PostItemViewModel _viewModel;

    public PostItemPage(PostItemViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Debug.WriteLine("PostItemPage appeared, BindingContext type: " + BindingContext?.GetType().FullName);
    }

}