using System.Diagnostics;
using Market.ViewModels.AddItem;
using Microsoft.Maui.Controls.Xaml;
using System.Linq;
using System.Threading.Tasks;
namespace Market.Views.AddItem
{
    // Add this attribute to force XAML compilation
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ForSaleItemPage : ContentPage
    {
        private readonly ForSaleItemViewModel _viewModel;
        private string _currentInput = string.Empty;

        public ForSaleItemPage(ForSaleItemViewModel viewModel)
        {
            try
            {
                InitializeComponent();
                _viewModel = viewModel;
                BindingContext = viewModel;
                Debug.WriteLine("ForSaleItemPage initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing ForSaleItemPage: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                // Handle photo permissions with safer error handling
#if IOS || MACCATALYST
                if (OperatingSystem.IsIOSVersionAtLeast(14, 0) || OperatingSystem.IsMacCatalystVersionAtLeast(14, 0))
                {
                    await RequestPhotoPermissions();
                }
                else
                {
                    await DisplayAlert("Unsupported OS Version",
                        "Photo access is not supported on this version of iOS or MacCatalyst.", "OK");
                }
#else
                await RequestPhotoPermissions();
#endif
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnAppearing: {ex.Message}");
                await DisplayAlert("Permission Error",
                    "There was an error checking photo permissions. Some features may not work correctly.", "OK");
            }
        }

        private async Task RequestPhotoPermissions()
        {
            try
            {
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
            catch (Exception ex)
            {
                Debug.WriteLine($"Error requesting photo permissions: {ex.Message}");
                throw; // Rethrow to the caller for handling
            }
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
}