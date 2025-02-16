using System.Diagnostics;
using Market.ViewModels;
using Market.ViewModels.AddItem;

namespace Market.Views.AddItem;

public partial class ServiceItemPage : ContentPage
{
    public ServiceItemPage(ServiceItemViewModel viewModel)
    {
        try
        {
            Console.WriteLine("ServiceItemPage: Constructor called");
            Debug.WriteLine("ServiceItemPage: Constructor called");

            InitializeComponent();
            BindingContext = viewModel;

            Console.WriteLine("ServiceItemPage: Initialization complete");
            Debug.WriteLine("ServiceItemPage: Initialization complete");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ServiceItemPage Constructor Error: {ex}");
            Debug.WriteLine($"ServiceItemPage Constructor Error: {ex}");
            throw;
        }
    }
}