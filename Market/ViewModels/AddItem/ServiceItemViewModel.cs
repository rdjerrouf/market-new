using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Market.Services;
using Market.DataAccess.Models;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions; // Add this for regex

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
        public List<string> ServiceCategories { get; } = new()
        {
            "Home & Garden",
            "Professional",
            "Technology",
            "Automotive",
            "Education & Tutoring",
            "Health & Wellness",
            "Creative & Design",
            "Events & Entertainment",
            "Other"
        };

        // Rate period options
        public List<string> RatePeriods { get; } = new()
        {
            "per Hour",
            "per Session",
            "per Project",
            "per Day"
        };

        #endregion

        #region Observable Properties

        /// <summary>
        /// Title of the service being offered
        /// </summary>
        private string _title = string.Empty; // Initialize
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        /// <summary>
        /// Detailed description of the service
        /// </summary>
        private string _description = string.Empty; // Initialize
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        /// <summary>
        /// Service rate/price
        /// </summary>
        private decimal _rate; // No need to initialize decimal

        public decimal Rate
        {
            get => _rate;
            set => SetProperty(ref _rate, value);
        }


        /// <summary>
        /// Selected service category
        /// </summary>
        private string _serviceCategory = string.Empty; // Initialize
        public string ServiceCategory
        {
            get => _serviceCategory;
            set => SetProperty(ref _serviceCategory, value);
        }

        /// <summary>
        /// Selected rate period
        /// </summary>
        private string _ratePeriod = string.Empty; // Initialize
        public string RatePeriod
        {
            get => _ratePeriod;
            set => SetProperty(ref _ratePeriod, value);
        }

        /// <summary>
        /// Additional details about service provider experience
        /// </summary>
        private string? _experience;
        public string? Experience
        {
            get => _experience;
            set => SetProperty(ref _experience, value);
        }

        /// <summary>
        /// Service area or location details
        /// </summary>
        private string? _serviceArea;
        public string? ServiceArea
        {
            get => _serviceArea;
            set => SetProperty(ref _serviceArea, value);
        }

        /// <summary>
        /// Indicates if service provider is available remotely
        /// </summary>
        private bool _isRemoteAvailable;
        public bool IsRemoteAvailable
        {
            get => _isRemoteAvailable;
            set => SetProperty(ref _isRemoteAvailable, value);
        }

        /// <summary>
        /// Processing state indicator
        /// </summary>
        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        #endregion

        #region Error Properties

        /// <summary>
        /// Error message for service title validation
        /// </summary>
        private string? _titleError;
        public string? TitleError
        {
            get => _titleError;
            set => SetProperty(ref _titleError, value);
        }

        /// <summary>
        /// Error message for service description validation
        /// </summary>
        private string? _descriptionError;
        public string? DescriptionError
        {
            get => _descriptionError;
            set => SetProperty(ref _descriptionError, value);
        }

        /// <summary>
        /// Error message for rate validation
        /// </summary>
        private string? _rateError;
        public string? RateError
        {
            get => _rateError;
            set => SetProperty(ref _rateError, value);
        }

        /// <summary>
        /// Error message for category validation
        /// </summary>
        private string? _categoryError;
        public string? CategoryError
        {
            get => _categoryError;
            set => SetProperty(ref _categoryError, value);
        }

        #endregion

        #region Computed Properties

        /// <summary>
        /// Indicates if the form can be submitted
        /// </summary>
        public bool CanSave => !IsBusy &&
                            !HasTitleError &&
                            !HasDescriptionError &&
                            !HasRateError &&
                            !HasCategoryError;

        /// <summary>
        /// UI state indicators for error messages
        /// </summary>
        public bool HasTitleError => !string.IsNullOrEmpty(TitleError);
        public bool HasDescriptionError => !string.IsNullOrEmpty(DescriptionError);
        public bool HasRateError => !string.IsNullOrEmpty(RateError);
        public bool HasCategoryError => !string.IsNullOrEmpty(CategoryError);

        #endregion

        /// <summary>
        /// Initializes a new instance of ServiceItemViewModel
        /// </summary>
        public ServiceItemViewModel(IItemService itemService, IAuthService authService)
        {
            _itemService = itemService ?? throw new ArgumentNullException(nameof(itemService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));

            // Initialize all properties with default values (CS8618 Fix)
            Title = string.Empty;
            Description = string.Empty;
            Rate = 0m;
            ServiceCategory = ServiceCategories[0];
            RatePeriod = RatePeriods[0];
            Experience = null;
            ServiceArea = null;
            IsRemoteAvailable = false;
            IsBusy = false;

            // Initialize error properties
            TitleError = null;
            DescriptionError = null;
            RateError = null;
            CategoryError = null;

            Debug.WriteLine("ServiceItemViewModel initialized");
        }

        #region Validation Methods

        /// <summary>
        /// Validates the service title
        /// </summary>
        private bool ValidateTitle()
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                TitleError = "Please enter a service title";
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

        /// <summary>
        /// Validates the service description
        /// </summary>
        private bool ValidateDescription()
        {
            if (string.IsNullOrWhiteSpace(Description))
            {
                DescriptionError = "Please describe your service";
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

        /// <summary>
        /// Validates the service rate
        /// </summary>
        private bool ValidateRate()
        {
            if (Rate < MIN_RATE)
            {
                RateError = "Rate must be greater than zero";
                return false;
            }

            if (Rate > MAX_RATE)
            {
                RateError = $"Rate cannot exceed {MAX_RATE:C}";
                return false;
            }

            RateError = null;
            return true;
        }

        /// <summary>
        /// Validates the service category selection
        /// </summary>
        private bool ValidateCategory()
        {
            if (string.IsNullOrEmpty(ServiceCategory))
            {
                CategoryError = "Please select a service category";
                return false;
            }

            CategoryError = null;
            return true;
        }

        #endregion

        #region Commands

        /// <summary>
        /// Command to save the service listing
        /// </summary>
        [RelayCommand]
        private async Task Save()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                Debug.WriteLine("Starting service listing save process");

                if (!ValidateAll())
                {
                    return;
                }

                var service = new Item
                {
                    Title = Title,
                    Description = BuildFullDescription(),
                    Price = Rate,
                    Category = "Services",
                    ServiceType = ServiceCategory,
                    RentalPeriod = RatePeriod,
                    ListedDate = DateTime.UtcNow,
                    UserId = await _authService.GetCurrentUserIdAsync()
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

        /// <summary>
        /// Validates all form fields
        /// </summary>
        private bool ValidateAll()
        {
            return ValidateTitle()
                   && ValidateDescription()
                   && ValidateRate()
                   && ValidateCategory();
        }

        /// <summary>
        /// Builds a complete description including experience and service area
        /// </summary>
        private string BuildFullDescription()
        {
            var fullDescription = new System.Text.StringBuilder();
            fullDescription.AppendLine(Description);

            if (!string.IsNullOrEmpty(Experience))
            {
                fullDescription.AppendLine();
                fullDescription.AppendLine("Experience:");
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