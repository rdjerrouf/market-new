using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using Market.Services;
using Market.DataAccess.Models;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Net.Mail;
using System.Text;

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

        // Apply Method Options
        public List<ApplyMethod> ApplyMethods { get; } = Enum.GetValues(typeof(ApplyMethod))
            .Cast<ApplyMethod>()
            .ToList();

        // Job Categories
        public List<JobCategory> JobCategories { get; } = Enum.GetValues(typeof(JobCategory))
            .Cast<JobCategory>()
            .ToList();

        // Add MinimumDate property
        public DateTime MinimumDate => DateTime.Today;
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

        private string _employmentType = string.Empty;
        public string EmploymentType
        {
            get => _employmentType;
            set => SetProperty(ref _employmentType, value);
        }

        private string _salaryPeriod = string.Empty;
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

        private ApplyMethod _selectedApplyMethod;
        public ApplyMethod SelectedApplyMethod
        {
            get => _selectedApplyMethod;
            set => SetProperty(ref _selectedApplyMethod, value);
        }

        private string _applyContact = string.Empty;
        public string ApplyContact
        {
            get => _applyContact;
            set => SetProperty(ref _applyContact, value);
        }

        private string? _applyContactError;
        public string? ApplyContactError
        {
            get => _applyContactError;
            set => SetProperty(ref _applyContactError, value);
        }

        private string _companyName = string.Empty;
        public string CompanyName
        {
            get => _companyName;
            set => SetProperty(ref _companyName, value);
        }

        private string? _companyNameError;
        public string? CompanyNameError
        {
            get => _companyNameError;
            set => SetProperty(ref _companyNameError, value);
        }

        private string _jobLocation = string.Empty;
        public string JobLocation
        {
            get => _jobLocation;
            set => SetProperty(ref _jobLocation, value);
        }

        private string? _jobLocationError;
        public string? JobLocationError
        {
            get => _jobLocationError;
            set => SetProperty(ref _jobLocationError, value);
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

        private JobCategory _selectedJobCategory;
        public JobCategory SelectedJobCategory
        {
            get => _selectedJobCategory;
            set => SetProperty(ref _selectedJobCategory, value);
        }
        #endregion

        #region Computed Properties

        public bool HasTitleError => !string.IsNullOrEmpty(TitleError);
        public bool HasDescriptionError => !string.IsNullOrEmpty(DescriptionError);
        public bool HasSalaryError => !string.IsNullOrEmpty(SalaryError);
        public bool HasEmploymentTypeError => !string.IsNullOrEmpty(EmploymentTypeError);
        public bool HasDateError => !string.IsNullOrEmpty(DateError);
        public bool HasApplyContactError => !string.IsNullOrEmpty(ApplyContactError);
        public bool HasCompanyNameError => !string.IsNullOrEmpty(CompanyNameError);
        public bool HasJobLocationError => !string.IsNullOrEmpty(JobLocationError);

        public bool CanSave => !IsBusy &&
                               !HasTitleError &&
                               !HasDescriptionError &&
                               !HasSalaryError &&
                               !HasEmploymentTypeError &&
                               !HasDateError &&
                               !HasCompanyNameError &&
                               !HasJobLocationError &&
                               !HasApplyContactError;

        #endregion

        public JobItemViewModel(IItemService itemService, IAuthService authService)
        {
            _itemService = itemService ?? throw new ArgumentNullException(nameof(itemService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));

            // Initialize defaults
            EmploymentType = EmploymentTypes[0];
            SalaryPeriod = SalaryPeriods[0];
            SelectedJobCategory = JobCategories[0];
            SelectedApplyMethod = ApplyMethods[0];
            StartDate = DateTime.Today;

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

        private bool ValidateCompanyName()
        {
            if (string.IsNullOrWhiteSpace(CompanyName))
            {
                CompanyNameError = "Please enter a company name";
                return false;
            }

            if (CompanyName.Length < 2)
            {
                CompanyNameError = "Company name must be at least 2 characters";
                return false;
            }

            CompanyNameError = null;
            return true;
        }

        private bool ValidateJobLocation()
        {
            if (string.IsNullOrWhiteSpace(JobLocation))
            {
                JobLocationError = "Please enter a job location";
                return false;
            }

            if (JobLocation.Length < 3)
            {
                JobLocationError = "Job location must be at least 3 characters";
                return false;
            }

            JobLocationError = null;
            return true;
        }

        private bool ValidateApplyContact()
        {
            if (string.IsNullOrWhiteSpace(ApplyContact))
            {
                ApplyContactError = "Please enter contact information";
                return false;
            }

            switch (SelectedApplyMethod)
            {
                case ApplyMethod.Email:
                    if (!IsValidEmail(ApplyContact))
                    {
                        ApplyContactError = "Please enter a valid email address";
                        return false;
                    }
                    break;
                case ApplyMethod.PhoneNumber:
                    if (!IsValidPhoneNumber(ApplyContact))
                    {
                        ApplyContactError = "Please enter a valid phone number";
                        return false;
                    }
                    break;
                case ApplyMethod.URL:
                    if (!IsValidUrl(ApplyContact))
                    {
                        ApplyContactError = "Please enter a valid URL";
                        return false;
                    }
                    break;
            }

            ApplyContactError = null;
            return true;
        }

        private bool ValidateAll()
        {
            return ValidateTitle()
                   && ValidateDescription()
                   && ValidateSalary()
                   && ValidateEmploymentType()
                   && ValidateStartDate()
                   && ValidateCompanyName()
                   && ValidateJobLocation()
                   && ValidateApplyContact();
        }

        #endregion

        #region Helper Methods

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPhoneNumber(string phoneNumber)
        {
            return Regex.IsMatch(
                phoneNumber,
                @"^[\+]?[(]?[0-9]{3}[)]?[-\s\.]?[0-9]{3}[-\s\.]?[0-9]{4,6}$"
            );
        }

        private bool IsValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out Uri? uriResult)
                   && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        private string BuildFullDescription()
        {
            var fullDescription = new StringBuilder();
            fullDescription.AppendLine(Description);
            fullDescription.AppendLine();
            fullDescription.AppendLine($"Employment Type: {EmploymentType}");
            fullDescription.AppendLine($"Salary: {Salary:C} {SalaryPeriod}");
            fullDescription.AppendLine($"Start Date: {StartDate:d}");
            fullDescription.AppendLine();
            fullDescription.AppendLine("How to Apply:");
            fullDescription.AppendLine($"Method: {SelectedApplyMethod}");
            fullDescription.AppendLine($"Contact: {ApplyContact}");

            return fullDescription.ToString();
        }

        #endregion

        #region Save
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
                    Description = BuildFullDescription(),
                    Price = Salary,
                    Category = "Jobs",
                    JobType = EmploymentType,
                    RentalPeriod = SalaryPeriod,
                    AvailableFrom = StartDate,
                    AvailableTo = StartDate.AddMonths(6), // Default 6-month listing
                    ListedDate = DateTime.UtcNow,
                    UserId = await _authService.GetCurrentUserIdAsync(),

                    // Job-specific properties
                    JobCategory = SelectedJobCategory,
                    CompanyName = CompanyName,
                    JobLocation = JobLocation,
                    ApplyMethod = SelectedApplyMethod,
                    ApplyContact = ApplyContact
                };

                Debug.WriteLine($"Saving job: {job.Title}, Category: {job.JobCategory}, Apply Method: {job.ApplyMethod}");
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
    }
}