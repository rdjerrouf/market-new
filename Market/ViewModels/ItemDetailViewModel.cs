using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Market.DataAccess.Models;
using System.Diagnostics;

namespace Market.ViewModels
{
    /// <summary>
    /// ViewModel for the item detail page, handling display and interaction with a marketplace item
    /// </summary>
    [QueryProperty(nameof(Item), "Item")]
    public partial class ItemDetailViewModel : ObservableObject
    {
        private Item? item;
        /// <summary>
        /// The marketplace item being displayed
        /// </summary>
        public Item? Item
        {
            get => item;
            set
            {
                if (SetProperty(ref item, value))
                {
                    OnItemSet();
                }
            }
        }

        /// <summary>
        /// Handles setup when an item is assigned
        /// </summary>
        private void OnItemSet()
        {
            if (Item != null)
            {
                Title = Item.Title;
            }
        }

        private string title = string.Empty;
        /// <summary>
        /// Title displayed in the page header
        /// </summary>
        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        /// <summary>
        /// Handles the contact seller action
        /// </summary>
        [RelayCommand]
        private async Task ContactSeller()
        {
            if (Item == null) return;

            try
            {
                Debug.WriteLine($"Contacting seller for item: {Item.Id}");
                // Navigate to chat/message page or show contact info
                await Shell.Current.DisplayAlert("Contact", "This feature is coming soon!", "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in ContactSeller: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Unable to contact seller at this time", "OK");
            }
        }
    }
}