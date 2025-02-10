using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Market.Services;
using Market.DataAccess.Models;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Market.ViewModels.AddItem
{
    public partial class JobItemViewModel : ObservableObject
    {
        #region Fields and Constants

        private readonly IItemService _itemService;
        private readonly IAuthService _authService;

        // Validation constants
        private const int TITLE_MIN_LENGTH = 5;
        private const int DESCRIPTION_MIN_LENGTH = 30;
        private const decimal MIN_SALARY = 0.01m;
        private const decimal MAX_SALARY = 999999.99m;

        // Employment type options
        public List<string> EmploymentTypes { get; } = new List<string>
        {
            "Full-time",
            "Part-time",
            "Contract",
            "Temporary",
            "Internship"
        };

        // Salary period options
        public List<string> SalaryPeriods { get; } = new List<string>
        {
            "per Hour",
            "per Week",
            "per Month",
            "per Year"
        };

        #endregion

        #region Observable Properties

        private string _title = string.Empty;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private string _description = string.Empty;
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        private decimal _salary;
        public decimal Salary
        {
            get => _salary;
            set => SetProperty(ref _salary, value);
        }

        private string _employmentType;
        public string EmploymentType
        {
            get => _employmentType;
            set => SetProperty(ref _employmentType, value);
        }

        private string _salaryPeriod;
        public string SalaryPeriod
        {
            get => _salaryPeriod;
            set => SetProperty(ref _salaryPeriod, value);
        }

        private DateTime _startDate = DateTime.Today;
        public DateTime StartDate
        {
            get => _startDate;
            set => SetProperty(ref _startDate, value);
        }

        private string? _titleError;
        public string? TitleError
        {
            get => _titleError;
            set => SetProperty(ref _titleError, value);
        }

        private string? _descriptionError;
        public string? DescriptionError
        {
            get => _descriptionError;
            set => SetProperty(ref _descriptionError, value);
        }

        private string? _salaryError;
        public string? SalaryError
        {
            get => _salaryError;
            set => SetProperty(ref _salaryError, value);
        }

        private string? _employmentTypeError;
        public string? EmploymentTypeError
        {
            get => _employmentTypeError;
            set => SetProperty(ref _employmentTypeError, value);
        }

        private string? _dateError;
        public string? DateError
        {
            get => _dateError;
            set => SetProperty(ref _dateError, value);
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        #endregion

        #region Computed Properties

        public bool CanSave => !IsBusy &&
                              !HasTitleError &&
                              !HasDescriptionError &&
                              !HasSalaryError &&
                              !HasEmploymentTypeError &&
                              !HasDateError;

        public bool HasTitleError => !string.IsNullOrEmpty(TitleError);
        public bool HasDescriptionError => !string.IsNullOrEmpty(DescriptionError);
        public bool HasSalaryError => !string.IsNullOrEmpty(SalaryError);
        public bool HasEmploymentTypeError => !string.IsNullOrEmpty(EmploymentTypeError);
        public bool HasDateError => !string.IsNullOrEmpty(DateError);
        public bool IsNotBusy => !IsBusy;

        #endregion

        public JobItemViewModel(IItemService itemService, IAuthService authService)
        {
            _itemService = itemService ?? throw new ArgumentNullException(nameof(itemService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));

            EmploymentType = EmploymentTypes[0];
            SalaryPeriod = SalaryPeriods[0];
            _employmentType = EmploymentTypes[0];
            _salaryPeriod = SalaryPeriods[0];


            Debug.WriteLine("JobItemViewModel initialized");
        }

        #region Validation Methods

        private bool ValidateTitle()
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                TitleError = "Please enter a job title";
                return false;
            }

            if (Title.Length < TITLE_MIN_LENGTH)
            {
                TitleError = $"Title must be at least {TITLE_MIN_LENGTH} characters";
                return false;
            }

            TitleError = null;
            return true;
        }

        private bool ValidateDescription()
        {
            if (string.IsNullOrWhiteSpace(Description))
            {
                DescriptionError = "Please enter a job description";
                return false;
            }

            if (Description.Length < DESCRIPTION_MIN_LENGTH)
            {
                DescriptionError = $"Description must be at least {DESCRIPTION_MIN_LENGTH} characters";
                return false;
            }

            DescriptionError = null;
            return true;
        }

        private bool ValidateSalary()
        {
            if (Salary < MIN_SALARY)
            {
                SalaryError = "Salary must be greater than zero";
                return false;
            }

            if (Salary > MAX_SALARY)
            {
                SalaryError = $"Salary cannot exceed {MAX_SALARY:C}";
                return false;
            }

            SalaryError = null;
            return true;
        }

        private bool ValidateEmploymentType()
        {
            if (string.IsNullOrEmpty(EmploymentType))
            {
                EmploymentTypeError = "Please select an employment type";
                return false;
            }

            EmploymentTypeError = null;
            return true;
        }

        private bool ValidateStartDate()
        {
            if (StartDate < DateTime.Today)
            {
                DateError = "Start date cannot be in the past";
                return false;
            }

            DateError = null;
            return true;
        }

        #endregion

        #region Save Command

        [RelayCommand]
        private async Task Save()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                Debug.WriteLine("Starting job listing save process");

                if (!ValidateAll())
                {
                    return;
                }

                var job = new Item
                {
                    Title = Title,
                    Description = Description,
                    Price = Salary,
                    Category = "Jobs",
                    JobType = EmploymentType,
                    RentalPeriod = SalaryPeriod,
                    AvailableFrom = StartDate,
                    ListedDate = DateTime.UtcNow,
                    UserId = await _authService.GetCurrentUserIdAsync()
                };

                Debug.WriteLine($"Saving job: {job.Title}, {job.Price} {job.RentalPeriod}");
                var result = await _itemService.AddItemAsync(job);

                if (result)
                {
                    await Shell.Current.DisplayAlert("Success", "Your job has been posted!", "OK");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to post job", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Save error: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "An error occurred while saving", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        #endregion

        #region Helper Methods

        private bool ValidateAll()
        {
            return ValidateTitle()
                   && ValidateDescription()
                   && ValidateSalary()
                   && ValidateEmploymentType()
                   && ValidateStartDate();
        }

        #endregion
    }
}