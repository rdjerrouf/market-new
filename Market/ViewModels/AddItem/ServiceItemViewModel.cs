using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Market.Services;
using Market.DataAccess.Models;
using System.Diagnostics;
using System.Text;

namespace Market.ViewModels.AddItem
{
    public partial class ServiceItemViewModel : ObservableObject
    {
        #region Fields and Constants

        private readonly IItemService _itemService;
        private readonly IAuthService _authService;

        // Validation constants
        private const int TITLE_MIN_LENGTH = 5;
        private const int DESCRIPTION_MIN_LENGTH = 30;
        private const decimal MIN_RATE = 0.01m;
        private const decimal MAX_RATE = 999999.99m;

        // Service categories
        public List<ServiceCategory> ServiceCategories
        {
            get
            {
                try
                {
                    var categories = Enum.GetValues(typeof(ServiceCategory))
                        .Cast<ServiceCategory>()
                        .ToList();

                    // Add detailed logging
                    Debug.WriteLine("ServiceCategories:");
                    foreach (var category in categories)
                    {
                        Debug.WriteLine($"- {category} (value: {(int)category})");
                    }

                    return categories;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error getting ServiceCategories: {ex}");
                    return new List<ServiceCategory>();
                }
            }
        }
        // Rate period options
        public List<string> RatePeriods { get; } = new()
        {
            "per Hour",
            "per Session",
            "per Project",
            "per Day"
        };

        // Service availability options
        public List<ServiceAvailability> ServiceAvailabilities
        {
            get
            {
                try
                {
                    var availabilities = Enum.GetValues(typeof(ServiceAvailability))
                        .Cast<ServiceAvailability>()
                        .ToList();

                    // Add detailed logging
                    Debug.WriteLine("ServiceAvailabilities:");
                    foreach (var availability in availabilities)
                    {
                        Debug.WriteLine($"- {availability} (value: {(int)availability})");
                    }

                    return availabilities;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error getting ServiceAvailabilities: {ex}");
                    return new List<ServiceAvailability>();
                }
            }
        }
        #endregion

        #region Observable Properties

        private string _title = string.Empty;
        public string Title
        {
            get => _title;
            set
            {
                if (SetProperty(ref _title, value))
                {
                    ValidateTitle();
                    OnPropertyChanged(nameof(CanSave));
                }
            }
        }

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

        private decimal _rate;
        public decimal Rate
        {
            get => _rate;
            set
            {
                if (SetProperty(ref _rate, value))
                {
                    ValidateRate();
                    OnPropertyChanged(nameof(CanSave));
                }
            }
        }

        private ServiceCategory _selectedServiceCategory;
        public ServiceCategory SelectedServiceCategory
        {
            get => _selectedServiceCategory;
            set
            {
                if (SetProperty(ref _selectedServiceCategory, value))
                {
                    Debug.WriteLine($"SelectedServiceCategory changed:");
                    Debug.WriteLine($"- New Value: {value}");
                    Debug.WriteLine($"- Enum Value: {(int)value}");

                    // Additional validation or logic if needed
                    ValidateCategory();
                    OnPropertyChanged(nameof(CanSave));
                }
            }
        }

        private string _ratePeriod = string.Empty;
        public string RatePeriod
        {
            get => _ratePeriod;
            set
            {
                Debug.WriteLine($"RatePeriod setter called:");
                Debug.WriteLine($"- Previous Value: {_ratePeriod}");
                Debug.WriteLine($"- New Value: {value}");

                if (SetProperty(ref _ratePeriod, value))
                {
                    Debug.WriteLine("RatePeriod value changed successfully");
                    OnPropertyChanged(nameof(CanSave));
                }
            }
        }

        private string? _experience;
        public string? Experience
        {
            get => _experience;
            set => SetProperty(ref _experience, value);
        }

        private string? _serviceArea;
        public string? ServiceArea
        {
            get => _serviceArea;
            set => SetProperty(ref _serviceArea, value);
        }

        private bool _isRemoteAvailable;
        public bool IsRemoteAvailable
        {
            get => _isRemoteAvailable;
            set => SetProperty(ref _isRemoteAvailable, value);
        }

        // New properties from previous update
        private int _yearsOfExperience;
        public int YearsOfExperience
        {
            get => _yearsOfExperience;
            set => SetProperty(ref _yearsOfExperience, value);
        }

        private int _numberOfEmployees;
        public int NumberOfEmployees
        {
            get => _numberOfEmployees;
            set => SetProperty(ref _numberOfEmployees, value);
        }

        private string _serviceLocation = string.Empty;
        public string ServiceLocation
        {
            get => _serviceLocation;
            set => SetProperty(ref _serviceLocation, value);
        }

        private ServiceAvailability _selectedServiceAvailability;
        public ServiceAvailability SelectedServiceAvailability
        {
            get => _selectedServiceAvailability;
            set
            {
                if (SetProperty(ref _selectedServiceAvailability, value))
                {
                    Debug.WriteLine($"SelectedServiceAvailability changed:");
                    Debug.WriteLine($"- New Value: {value}");
                    Debug.WriteLine($"- Enum Value: {(int)value}");

                    OnPropertyChanged(nameof(CanSave));
                }
            }
        }
        // Error properties
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

        private string? _rateError;
        public string? RateError
        {
            get => _rateError;
            set => SetProperty(ref _rateError, value);
        }

        private string? _categoryError;
        public string? CategoryError
        {
            get => _categoryError;
            set => SetProperty(ref _categoryError, value);
        }

        // New error properties
        private string? _yearsOfExperienceError;
        public string? YearsOfExperienceError
        {
            get => _yearsOfExperienceError;
            set => SetProperty(ref _yearsOfExperienceError, value);
        }

        private string? _numberOfEmployeesError;
        public string? NumberOfEmployeesError
        {
            get => _numberOfEmployeesError;
            set => SetProperty(ref _numberOfEmployeesError, value);
        }

        private string? _serviceLocationError;
        public string? ServiceLocationError
        {
            get => _serviceLocationError;
            set => SetProperty(ref _serviceLocationError, value);
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        #endregion

        #region Computed Properties

        public bool CanSave
        {
            get
            {
                bool titleValid = !string.IsNullOrWhiteSpace(Title) && Title.Length >= 2;
                bool descriptionValid = !string.IsNullOrWhiteSpace(Description) && Description.Length >= 10;
                bool rateValid = Rate > 0;

                var canSave = !IsBusy &&
                              titleValid &&
                              descriptionValid &&
                              rateValid;

                Debug.WriteLine($"CanSave evaluation:");
                Debug.WriteLine($"  IsBusy: {IsBusy}");
                Debug.WriteLine($"  Title Valid: {titleValid} ('{Title}')");
                Debug.WriteLine($"  Description Valid: {descriptionValid} (length: {Description?.Length ?? 0})");
                Debug.WriteLine($"  Rate Valid: {rateValid} (value: {Rate})");
                Debug.WriteLine($"  Final result: {canSave}");

                return canSave;
            }
        }

        public bool HasTitleError => !string.IsNullOrEmpty(TitleError);
        public bool HasDescriptionError => !string.IsNullOrEmpty(DescriptionError);
        public bool HasRateError => !string.IsNullOrEmpty(RateError);
        public bool HasCategoryError => !string.IsNullOrEmpty(CategoryError);
        public bool HasYearsOfExperienceError => !string.IsNullOrEmpty(YearsOfExperienceError);
        public bool HasNumberOfEmployeesError => !string.IsNullOrEmpty(NumberOfEmployeesError);
        public bool HasServiceLocationError => !string.IsNullOrEmpty(ServiceLocationError);

        #endregion

        public ServiceItemViewModel(IItemService itemService, IAuthService authService)
        {
            try
            {
                Debug.WriteLine("Starting ServiceItemViewModel initialization");

                _itemService = itemService ?? throw new ArgumentNullException(nameof(itemService));
                _authService = authService ?? throw new ArgumentNullException(nameof(authService));

                Debug.WriteLine($"ServiceCategories count: {ServiceCategories.Count}");
                Debug.WriteLine($"ServiceAvailabilities count: {ServiceAvailabilities.Count}");
                Debug.WriteLine($"RatePeriods count: {RatePeriods.Count}");

                // Initialize defaults with validation
                if (ServiceCategories.Count > 0)
                {
                    SelectedServiceCategory = ServiceCategories[0];
                    Debug.WriteLine($"Set default ServiceCategory: {SelectedServiceCategory}");
                }

                if (ServiceAvailabilities.Count > 0)
                {
                    SelectedServiceAvailability = ServiceAvailabilities[0];
                    Debug.WriteLine($"Set default ServiceAvailability: {SelectedServiceAvailability}");
                }

                if (RatePeriods.Count > 0)
                {
                    RatePeriod = RatePeriods[0];
                    Debug.WriteLine($"Set default RatePeriod: {RatePeriod}");
                }

                // Initialize other fields
                Rate = 0;
                Title = string.Empty;
                Description = string.Empty;
                ServiceLocation = string.Empty;
                YearsOfExperience = 0;
                NumberOfEmployees = 0;
                Experience = string.Empty;
                ServiceArea = string.Empty;
                IsRemoteAvailable = false;

                Debug.WriteLine("ServiceItemViewModel initialization completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in ServiceItemViewModel constructor: {ex}");
                throw;
            }
        }

        #region Validation Methods

        private bool ValidateTitle()
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                TitleError = "Please enter a service title";
                return false;
            }

            if (Title.Length < 2)
            {
                TitleError = "Title must be at least 2 characters";
                return false;
            }

            TitleError = null;
            return true;
        }

        private bool ValidateDescription()
        {
            if (string.IsNullOrWhiteSpace(Description))
            {
                DescriptionError = "Please describe your service";
                return false;
            }

            if (Description.Length < 10)
            {
                DescriptionError = "Description must be at least 10 characters";
                return false;
            }

            DescriptionError = null;
            return true;
        }

        private bool ValidateRate()
        {
            if (Rate <= 0)
            {
                RateError = "Please enter a valid rate";
                return false;
            }

            RateError = null;
            return true;
        }

        private bool ValidateCategory()
        {
            CategoryError = null;
            return true;
        }

        private bool ValidateYearsOfExperience()
        {
            if (YearsOfExperience < 0)
            {
                YearsOfExperienceError = "Years of experience cannot be negative";
                return false;
            }

            if (YearsOfExperience > 50)
            {
                YearsOfExperienceError = "Years of experience seems unrealistic";
                return false;
            }

            YearsOfExperienceError = null;
            return true;
        }

        private bool ValidateNumberOfEmployees()
        {
            if (NumberOfEmployees < 0)
            {
                NumberOfEmployeesError = "Number of employees cannot be negative";
                return false;
            }

            if (NumberOfEmployees > 1000)
            {
                NumberOfEmployeesError = "Number of employees seems unrealistic";
                return false;
            }

            NumberOfEmployeesError = null;
            return true;
        }

        private bool ValidateServiceLocation()
        {
            if (string.IsNullOrWhiteSpace(ServiceLocation))
            {
                ServiceLocationError = "Please enter a service location";
                return false;
            }

            if (ServiceLocation.Length < 3)
            {
                ServiceLocationError = "Service location must be at least 3 characters";
                return false;
            }

            ServiceLocationError = null;
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
                Debug.WriteLine("Starting service listing save process");

                // Remove strict validations
                if (string.IsNullOrWhiteSpace(Title) ||
                    string.IsNullOrWhiteSpace(Description) ||
                    Rate <= 0)
                {
                    Debug.WriteLine("Validation failed. Cannot save.");
                    return;
                }

                var service = new Item
                {
                    Title = Title,
                    Description = BuildFullDescription(),
                    Price = Rate,
                    Category = "Services",
                    ServiceType = SelectedServiceCategory.ToString(),
                    RentalPeriod = RatePeriod,
                    ListedDate = DateTime.UtcNow,
                    UserId = await _authService.GetCurrentUserIdAsync(),
                    ServiceCategory = SelectedServiceCategory,
                    ServiceAvailability = SelectedServiceAvailability,
                    ServiceLocation = ServiceLocation,
                    YearsOfExperience = YearsOfExperience,
                    NumberOfEmployees = NumberOfEmployees
                };

                Debug.WriteLine($"Saving service: {service.Title}, {service.Price} {service.RentalPeriod}");
                var result = await _itemService.AddItemAsync(service);

                if (result)
                {
                    await Shell.Current.DisplayAlert("Success", "Your service has been posted!", "OK");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to post service", "OK");
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
            return true;
        }

        private string BuildFullDescription()
        {
            var fullDescription = new StringBuilder();
            fullDescription.AppendLine(Description);

            fullDescription.AppendLine();
            fullDescription.AppendLine($"Years of Experience: {YearsOfExperience}");
            fullDescription.AppendLine($"Number of Employees: {NumberOfEmployees}");
            fullDescription.AppendLine($"Service Location: {ServiceLocation}");
            fullDescription.AppendLine($"Availability: {SelectedServiceAvailability}");

            if (!string.IsNullOrEmpty(Experience))
            {
                fullDescription.AppendLine();
                fullDescription.AppendLine("Additional Experience:");
                fullDescription.AppendLine(Experience);
            }

            if (!string.IsNullOrEmpty(ServiceArea))
            {
                fullDescription.AppendLine();
                fullDescription.AppendLine("Service Area:");
                fullDescription.AppendLine(ServiceArea);
            }

            if (IsRemoteAvailable)
            {
                fullDescription.AppendLine();
                fullDescription.AppendLine("✓ Remote service available");
            }

            return fullDescription.ToString();
        }

        #endregion
    }
}