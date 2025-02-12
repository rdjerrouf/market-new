using System.Diagnostics;
using Market.ViewModels.AddItem;

namespace Market.Views.AddItem;

public partial class JobItemPage : ContentPage
{
    public JobItemPage(JobItemViewModel viewModel)
    {
        try
        {
            Debug.WriteLine("Initializing JobItemPage");
            BindingContext = viewModel;
            InitializeComponent();
            Debug.WriteLine("JobItemPage initialization completed");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error initializing JobItemPage: {ex}");
            throw;
        }
    }
}