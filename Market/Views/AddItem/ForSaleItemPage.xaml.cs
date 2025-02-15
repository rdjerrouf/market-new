using System.Diagnostics;
using Market.ViewModels.AddItem;
namespace Market.Views.AddItem;

public partial class ForSaleItemPage : ContentPage
{
    private readonly ForSaleItemViewModel _viewModel;
    private string _currentInput = string.Empty;

    public ForSaleItemPage(ForSaleItemViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
#if IOS || MACCATALYST
        if (OperatingSystem.IsIOSVersionAtLeast(14, 0) || OperatingSystem.IsMacCatalystVersionAtLeast(14, 0))
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Photos>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Photos>();
                if (status != PermissionStatus.Granted)
                {
                    await DisplayAlert("Permission Required",
                        "Photo access is required to upload photos.", "OK");
                }
            }
        }
        else
        {
            await DisplayAlert("Unsupported OS Version",
                "Photo access is not supported on this version of iOS or MacCatalyst.", "OK");
        }
#else
        var status = await Permissions.CheckStatusAsync<Permissions.Photos>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.Photos>();
            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("Permission Required",
                    "Photo access is required to upload photos.", "OK");
            }
        }
#endif
    }

    private void OnPriceTextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is Entry entry)
        {
            Debug.WriteLine($"Price text changed. New value: {e.NewTextValue}");

            string newText = e.NewTextValue;
            if (!string.IsNullOrEmpty(newText) &&
                !newText.All(c => char.IsDigit(c) || c == '.') ||
                newText.Count(c => c == '.') > 1)
            {
                entry.Text = e.OldTextValue;
                return;
            }
            _currentInput = newText;
        }
    }

    private void OnPriceCompleted(object sender, EventArgs e)
    {
        if (sender is Entry entry)
        {
            Debug.WriteLine($"Price input completed. Final value: {_currentInput}");
            if (decimal.TryParse(_currentInput, out decimal result))
            {
                _viewModel.Price = result;
                entry.Text = result.ToString("F2");
            }
        }
    }
}