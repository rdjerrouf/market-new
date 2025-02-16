using System.Diagnostics;
using Market.ViewModels.AddItem;
namespace Market.Views.AddItem;

public partial class JobItemPage : ContentPage
{
    private readonly JobItemViewModel _viewModel;
    private string _currentInput = string.Empty;

    public JobItemPage(JobItemViewModel viewModel)
    {
        try
        {
            Debug.WriteLine("Initializing JobItemPage");
            _viewModel = viewModel;
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

    private void OnSalaryTextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is Entry entry)
        {
            Debug.WriteLine($"Salary text changed. New value: {e.NewTextValue}");

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

    private void OnSalaryCompleted(object sender, EventArgs e)
    {
        if (sender is Entry entry)
        {
            Debug.WriteLine($"Salary input completed. Final value: {_currentInput}");
            if (decimal.TryParse(_currentInput, out decimal result))
            {
                _viewModel.Salary = result;
                entry.Text = result.ToString("F2");
            }
        }
    }

    private void OnApplyContactTextChanged(object sender, TextChangedEventArgs e)
    {
        // Ensure the binding updates and validation runs
        if (sender is Entry entry)
        {
            ((JobItemViewModel)BindingContext).ApplyContact = entry.Text;
        }
    }
}