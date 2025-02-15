using System.Diagnostics;
using Market.ViewModels.AddItem;
namespace Market.Views.AddItem;

public partial class RentalItemPage : ContentPage
{
    private readonly RentalItemViewModel _viewModel;

    public RentalItemPage(RentalItemViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    private async void OnAddPhotoClicked(object sender, EventArgs e)
    {
        if (_viewModel != null)
        {
            await _viewModel.AddPhoto();
        }
    }

    private string _currentInput = string.Empty;

    private void OnPriceTextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is Entry entry)
        {
            Debug.WriteLine($"Price text changed. New value: {e.NewTextValue}");

            // Only allow digits and one decimal point
            string newText = e.NewTextValue;
            if (!string.IsNullOrEmpty(newText) &&
                !newText.All(c => char.IsDigit(c) || c == '.') ||
                newText.Count(c => c == '.') > 1)
            {
                entry.Text = e.OldTextValue;
                return;
            }

            _currentInput = newText; // Store the current input
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