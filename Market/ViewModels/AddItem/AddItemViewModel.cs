using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace Market.ViewModels.AddItem
{
    /// <summary>
    /// ViewModel for the category selection page when adding new items
    /// Handles navigation to specific item creation pages based on category
    /// </summary>
    public partial class AddItemViewModel : ObservableObject
    {
        /// <summary>
        /// Navigates to the For Sale item creation page
        /// Used for items being sold outright
        /// </summary>
        [RelayCommand]
        private async Task ForSale()
        {
            try
            {
                Debug.WriteLine("Navigating to ForSaleItemPage");
                await Shell.Current.GoToAsync("ForSaleItemPage");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation error: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Unable to open For Sale form", "OK");
            }
        }

        /// <summary>
        /// Navigates to the Rental item creation page
        /// Used for property and item rentals with date ranges
        /// </summary>
        [RelayCommand]
        private async Task Rental()
        {
            try
            {
                Debug.WriteLine("Navigating to RentalItemPage");
                await Shell.Current.GoToAsync("RentalItemPage");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation error: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Unable to open Rental form", "OK");
            }
        }

        /// <summary>
        /// Navigates to the Job listing creation page
        /// Used for posting job opportunities
        /// </summary>
        [RelayCommand]
        private async Task Job()
        {
            try
            {
                Debug.WriteLine("Navigating to JobItemPage");
                await Shell.Current.GoToAsync("JobItemPage");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation error: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Unable to open Job form", "OK");
            }
        }

        /// <summary>
        /// Navigates to the Service listing creation page
        /// Used for offering services
        /// </summary>
        [RelayCommand]
        private async Task Service()
        {
            try
            {
                Debug.WriteLine("Navigating to ServiceItemPage");
                await Shell.Current.GoToAsync("ServiceItemPage");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation error: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Unable to open Service form", "OK");
            }
        }
    }
}