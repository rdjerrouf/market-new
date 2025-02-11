namespace Market.Views.AddItem;
using Market.ViewModels.AddItem;
public partial class ForSaleItemPage : ContentPage
{
    public ForSaleItemPage(ForSaleItemViewModel viewModel)
    {
        InitializeComponent();
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

}