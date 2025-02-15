using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Market.Converters;
using System.Threading.Tasks;
using Market.Services;
using Market.DataAccess.Models;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Net.Mail;
using System.Text;

namespace Market.ViewModels.AddItem
#pragma warning disable MVVMTK0045 // Observable property not AOT compatible
{
    public partial class JobItemViewModel : ObservableObject
    {
        #region Fields and Constants

        private readonly IItemService _itemService;
        private readonly IAuthService _authService;

        private const int TITLE_MIN_LENGTH = 2;
        private const int DESCRIPTION_MIN_LENGTH = 10;
        private const decimal MIN_SALARY = 0.01m;
        private const decimal MAX_SALARY = 9999999.99m;

        public List<string> EmploymentTypes { get; } =
        [
            "Full-time",
            "Part-time",
            "Contract",
            "Temporary",
            "Internship"
        ];

        public List<string> SalaryPeriods { get; } =
        [
            "per Hour",
            "per Week",
            "per Month",
            "per Year"
        ];

        private List<ApplyMethod> _applyMethods = [];
        public List<ApplyMethod> ApplyMethods
        {
            get
            {
                try
                {
                    _applyMethods ??= Enum.GetValues(typeof(ApplyMethod))
                        .Cast<ApplyMethod>()
                        .ToList();
                    return _applyMethods;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in ApplyMethods getter: {ex}");
                    return [];
                }
            }
        }

        private List<JobCategory> _jobCategories = [];
        public List<JobCategory> JobCategories
        {
            get
            {
                try
                {
                    if (_jobCategories.Count == 0)
                    {
                        Debug.WriteLine("Initializing JobCategories list");
                        _jobCategories = Enum.GetValues(typeof(JobCategory))
                            .Cast<JobCategory>()
                            .ToList();
                        Debug.WriteLine($"JobCategories initialized with {_jobCategories.Count} items");
                    }
                    return _jobCategories;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in JobCategories getter: {ex}");
                    return [];
                }
            }
        }

        public DateTime MinimumDate => DateTime.Today;
        #endregion

        #region Observable Properties

        [ObservableProperty]
        private string _title = string.Empty;

        private string _description = string.Empty;
        public string Description
        {
            get => _description;
            set
            {
                if (SetProperty(ref _description, value))
                {
                    ValidateDescription();
                    OnPropertyChanged(nameof(CanSave));
                }
            }
        }
        private decimal _salary;
        public decimal Salary
        {
            get => _salary;
            set
            {
                try
                {
                    if (SetProperty(ref _salary, value))
                    {
                        ValidateSalary();
                        OnPropertyChanged(nameof(CanSave));
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error setting salary: {ex.Message}");
                    SalaryError = "Please enter a valid number";
                }
            }
        }

        [ObservableProperty]
        private string _employmentType = string.Empty;

        [ObservableProperty]
        private string _salaryPeriod = string.Empty;

        [ObservableProperty]
        private DateTime _startDate = DateTime.Today;

        private ApplyMethod _selectedApplyMethod;
        public ApplyMethod SelectedApplyMethod
        {
            get => _selectedApplyMethod;
            set
            {
                if (SetProperty(ref _selectedApplyMethod, value))
                {
                    Debug.WriteLine($"SelectedApplyMethod changed to: {value}");
                    ValidateApplyContact();
                    OnPropertyChanged(nameof(HasApplyContactError));
                    OnPropertyChanged(nameof(CanSave));
                }
            }
        }

        private string _applyContact = string.Empty;
        public string ApplyContact
        {
            get => _applyContact;
            set
            {
                if (SetProperty(ref _applyContact, value))
                {
                    Debug.WriteLine($"ApplyContact changed to: '{value}'");
                    ValidateApplyContact();
                    OnPropertyChanged(nameof(HasApplyContactError));
                    OnPropertyChanged(nameof(CanSave));
                }
            }
        }

        [ObservableProperty]
        private string? _applyContactError;

        [ObservableProperty]
        private string _companyName = string.Empty;

        [ObservableProperty]
        private string? _companyNameError;

        [ObservableProperty]
        private string _jobLocation = string.Empty;

        [ObservableProperty]
        private string? _jobLocationError;

        [ObservableProperty]
        private string? _titleError;

        [ObservableProperty]
        private string? _descriptionError;

        [ObservableProperty]
        private string? _salaryError;

        [ObservableProperty]
        private string? _employmentTypeError;

        [ObservableProperty]
        private string? _dateError;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private JobCategory _selectedJobCategory;

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

        public bool CanSave
        {
            get
            {
                var canSave = !IsBusy &&
                              !HasTitleError &&
                              !HasDescriptionError &&
                              !HasSalaryError &&
                              !HasEmploymentTypeError &&
                              !HasDateError &&
                              !HasCompanyNameError &&
                              !HasJobLocationError &&
                              !HasApplyContactError;

                Debug.WriteLine($"CanSave evaluation:");
                Debug.WriteLine($"  IsBusy: {IsBusy}");
                Debug.WriteLine($"  HasTitleError: {HasTitleError}");
                Debug.WriteLine($"  HasDescriptionError: {HasDescriptionError}");
                Debug.WriteLine($"  HasSalaryError: {HasSalaryError}");
                Debug.WriteLine($"  HasEmploymentTypeError: {HasEmploymentTypeError}");
                Debug.WriteLine($"  HasDateError: {HasDateError}");
                Debug.WriteLine($"  HasCompanyNameError: {HasCompanyNameError}");
                Debug.WriteLine($"  HasJobLocationError: {HasJobLocationError}");
                Debug.WriteLine($"  HasApplyContactError: {HasApplyContactError}");
                Debug.WriteLine($"  Final result: {canSave}");

                return canSave;
            }
        }

        #endregion

        public JobItemViewModel(IItemService itemService, IAuthService authService)
        {
            try
            {
                Debug.WriteLine("Starting JobItemViewModel initialization");

                _itemService = itemService ?? throw new ArgumentNullException(nameof(itemService));
                _authService = authService ?? throw new ArgumentNullException(nameof(authService));

                // Initialize lists and defaults
                InitializeDefaults();

                Debug.WriteLine("JobItemViewModel initialization completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in JobItemViewModel constructor: {ex}");
                throw;
            }
        }

        private void InitializeDefaults()
        {
            Debug.WriteLine("Starting InitializeDefaults");

            // Initialize basic fields
            Title = string.Empty;
            Description = string.Empty;
            CompanyName = string.Empty;
            JobLocation = string.Empty;
            StartDate = DateTime.Today;

            // Set defaults and validate immediate
            if (JobCategories.Count > 0)
            {
                SelectedJobCategory = JobCategories[0];
                Debug.WriteLine($"Set default JobCategory: {SelectedJobCategory}");
            }

            if (ApplyMethods.Count > 0)
            {
                SelectedApplyMethod = ApplyMethods[0];
                Debug.WriteLine($"Set default ApplyMethod: {SelectedApplyMethod}");
            }

            if (EmploymentTypes.Count > 0)
            {
                EmploymentType = EmploymentTypes[0];
                Debug.WriteLine($"Set default EmploymentType: {EmploymentType}");
            }

            if (SalaryPeriods.Count > 0)
            {
                SalaryPeriod = SalaryPeriods[0];
                Debug.WriteLine($"Set default SalaryPeriod: {SalaryPeriod}");
            }

            // Clear the contact field and validate it
            ApplyContact = string.Empty;
            ValidateApplyContact();

            Debug.WriteLine("Initial validation state:");
            Debug.WriteLine($"ApplyContactError: {ApplyContactError}");
            Debug.WriteLine($"HasApplyContactError: {HasApplyContactError}");
            Debug.WriteLine($"CanSave: {CanSave}");
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
            try
            {
                if (Salary <= 0)
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
            catch (Exception ex)
            {
                Debug.WriteLine($"Error validating salary: {ex.Message}");
                SalaryError = "Invalid salary value";
                return false;
            }
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


        // Use the enum type for SelectedApplyMethod

        // Update the validation method to handle enum
        private bool ValidateApplyContact()
        {
            Debug.WriteLine($"Validating ApplyContact: '{ApplyContact}', Method: {SelectedApplyMethod}");

            // If the field is empty, that's okay during typing
            if (string.IsNullOrWhiteSpace(ApplyContact))
            {
                ApplyContactError = "Please enter contact information";
                return false;
            }

            // Only validate format if the field is not empty
            if (ApplyContact.Length > 3)  // Only validate once there's substantial input
            {
                bool isValid = SelectedApplyMethod switch
                {
                    ApplyMethod.Email => IsValidEmail(ApplyContact),
                    ApplyMethod.PhoneNumber => IsValidPhoneNumber(ApplyContact),
                    ApplyMethod.URL => IsValidUrl(ApplyContact),
                    _ => false
                };

                if (!isValid)
                {
                    ApplyContactError = $"Please enter a valid {SelectedApplyMethod.ToString().ToLower()}";
                    Debug.WriteLine($"ApplyContact validation failed: invalid {SelectedApplyMethod}");
                    return false;
                }
            }

            ApplyContactError = null;
            Debug.WriteLine("ApplyContact validation passed");
            return true;
        }

        private bool ValidateAll()
        {
            var validations = new Dictionary<string, bool>
            {
                { "Title", ValidateTitle() },
                { "Description", ValidateDescription() },
                { "Salary", ValidateSalary() },
                { "EmploymentType", ValidateEmploymentType() },
                { "StartDate", ValidateStartDate() },
                { "CompanyName", ValidateCompanyName() },
                { "JobLocation", ValidateJobLocation() },
                { "ApplyContact", ValidateApplyContact() }
            };

            foreach (var (field, isValid) in validations)
            {
                Debug.WriteLine($"Validation for {field}: {isValid}");
            }

            return validations.Values.All(v => v);
        }

        #endregion

        #region Helper Methods

        private static bool IsValidEmail(string email)
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

        [GeneratedRegex(@"^[\+]?[(]?[0-9]{3}[)]?[-\s\.]?[0-9]{3}[-\s\.]?[0-9]{4,6}$")]
        private static partial Regex PhoneRegex();

        private static bool IsValidPhoneNumber(string phoneNumber)
        {
            return PhoneRegex().IsMatch(phoneNumber);
        }

        private static bool IsValidUrl(string url)
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

        #region Save Command
        [RelayCommand(CanExecute = nameof(CanSave))]
        private async Task Save()
        {
            Debug.WriteLine("Save command initiated");
            Debug.WriteLine($"Current state - IsBusy: {IsBusy}, CanSave: {CanSave}");
            Debug.WriteLine($"Title: '{Title}', Description length: {Description?.Length ?? 0}");
            Debug.WriteLine($"Salary: {Salary}, Employment Type: {EmploymentType}");
            Debug.WriteLine($"Company: {CompanyName}, Location: {JobLocation}");
            Debug.WriteLine($"ApplyMethod: {SelectedApplyMethod}, Contact: {ApplyContact}");

            if (!CanSave)
            {
                Debug.WriteLine("Save canceled - validation errors present");
                return;
            }

            if (IsBusy)
            {
                Debug.WriteLine("Save canceled - IsBusy is true");
                return;
            }

            try
            {
                IsBusy = true;
                Debug.WriteLine("Starting job listing save process");

                Debug.WriteLine("Running all validations...");
                var validations = new Dictionary<string, bool>
        {
            { "Title", ValidateTitle() },
            { "Description", ValidateDescription() },
            { "Salary", ValidateSalary() },
            { "EmploymentType", ValidateEmploymentType() },
            { "StartDate", ValidateStartDate() },
            { "CompanyName", ValidateCompanyName() },
            { "JobLocation", ValidateJobLocation() },
            { "ApplyContact", ValidateApplyContact() }
        };

                Debug.WriteLine("Validation results:");
                foreach (var (field, isValid) in validations)
                {
                    Debug.WriteLine($"  {field}: {isValid}");
                    if (!isValid)
                    {
                        Debug.WriteLine($"    Error message: {GetErrorMessage(field)}");
                    }
                }

                if (!validations.Values.All(v => v))
                {
                    Debug.WriteLine("Validation failed - canceling save");
                    return;
                }

                Debug.WriteLine("All validations passed, creating job item");
                var jobItem = new Item
                {
                    Title = Title,
                    Description = BuildFullDescription(),
                    Price = Salary,
                    Category = "Jobs",
                    JobType = EmploymentType,
                    RentalPeriod = SalaryPeriod,
                    AvailableFrom = StartDate,
                    AvailableTo = StartDate.AddMonths(6),
                    ListedDate = DateTime.UtcNow,
                    UserId = await _authService.GetCurrentUserIdAsync(),
                    JobCategory = SelectedJobCategory,
                    CompanyName = CompanyName,
                    JobLocation = JobLocation,
                    ApplyMethod = SelectedApplyMethod,
                    ApplyContact = ApplyContact
                };

                Debug.WriteLine("Job item created with properties:");
                Debug.WriteLine($"  Title: {jobItem.Title}");
                Debug.WriteLine($"  Category: {jobItem.Category}");
                Debug.WriteLine($"  JobCategory: {jobItem.JobCategory}");
                Debug.WriteLine($"  Price: {jobItem.Price}");
                Debug.WriteLine($"  UserId: {jobItem.UserId}");

                Debug.WriteLine("Attempting to save job to database");
                var saveSuccessful = await _itemService.AddItemAsync(jobItem);
                Debug.WriteLine($"Save result: {saveSuccessful}");

                if (saveSuccessful)
                {
                    Debug.WriteLine("Save successful, showing success message");
                    await Shell.Current.DisplayAlert("Success", "Your job has been posted!", "OK");

                    // Clear the form fields
                    ClearForm();

                    Debug.WriteLine("Navigating to main page");
                    await Shell.Current.GoToAsync("//MainPage");
                }
                else
                {
                    Debug.WriteLine("Save failed, showing error message");
                    await Shell.Current.DisplayAlert("Error", "Failed to post job", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Save error: {ex}");
                Debug.WriteLine($"Error type: {ex.GetType().Name}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    Debug.WriteLine($"Inner stack trace: {ex.InnerException.StackTrace}");
                }
                await Shell.Current.DisplayAlert("Error", "An error occurred while saving", "OK");
            }
            finally
            {
                IsBusy = false;
                Debug.WriteLine("Save process completed");

                // Trigger CanSave re-evaluation
                OnPropertyChanged(nameof(CanSave));
            }
        }

        // Helper method to clear form fields
        private void ClearForm()
        {
            Title = string.Empty;
            Description = string.Empty;
            CompanyName = string.Empty;
            JobLocation = string.Empty;
            StartDate = DateTime.Today;
            Salary = 0;
            EmploymentType = EmploymentTypes.FirstOrDefault() ?? string.Empty;
            SalaryPeriod = SalaryPeriods.FirstOrDefault() ?? string.Empty;
            SelectedApplyMethod = ApplyMethods.FirstOrDefault();
            ApplyContact = string.Empty;
        }

        // Helper method to get error messages
        private string? GetErrorMessage(string field) => field switch
        {
            "Title" => TitleError,
            "Description" => DescriptionError,
            "Salary" => SalaryError,
            "EmploymentType" => EmploymentTypeError,
            "StartDate" => DateError,
            "CompanyName" => CompanyNameError,
            "JobLocation" => JobLocationError,
            "ApplyContact" => ApplyContactError,
            _ => null
        };
        #endregion
    }
}
#pragma warning restore MVVMTK0045