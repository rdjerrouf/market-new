using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Market.DataAccess.Models;
using Market.Services;

namespace Market.ViewModels
{
    /// <summary>
    /// Manages the inbox view, handling message retrieval, display, and interactions
    /// </summary>
    public partial class InboxViewModel : ObservableObject
    {
        // Services for message management and user authentication
        private readonly IMessageService _messageService;
        private readonly IAuthService _authService;

        /// <summary>
        /// Collection of messages to be displayed in the inbox
        /// Uses ObservableCollection for real-time UI updates
        /// </summary>
        public ObservableCollection<Message> Messages { get; } = new();

        /// <summary>
        /// Indicates whether messages are currently being loaded
        /// </summary>
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }        /// <summary>
                 /// Constructor for InboxViewModel
                 /// Initializes services and triggers message loading
                 /// </summary>
                 /// <param name="messageService">Service for managing messages</param>
                 /// <param name="authService">Service for user authentication</param>
        public InboxViewModel(IMessageService messageService, IAuthService authService)
        {
            _messageService = messageService;
            _authService = authService;
            LoadMessagesAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Loads messages for the current user
        /// Handles loading state and error scenarios
        /// </summary>
        private async Task LoadMessagesAsync()
        {
            // Prevent multiple simultaneous loading attempts
            if (IsLoading) return;

            try
            {
                // Indicate loading is in progress
                IsLoading = true;
                Debug.WriteLine("Loading inbox messages...");

                // TODO: Replace with actual user authentication
                // Placeholder user ID - replace with actual authentication mechanism
                int currentUserId = 1;

                // Retrieve messages for the current user
                var messages = await _messageService.GetUserInboxMessagesAsync(currentUserId);

                // Clear existing messages and populate with new data
                Messages.Clear();
                foreach (var message in messages)
                {
                    Messages.Add(message);
                }

                Debug.WriteLine($"Loaded {Messages.Count} messages");
            }
            catch (Exception ex)
            {
                // Log and display error if message loading fails
                Debug.WriteLine($"Error loading messages: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Unable to load messages", "OK");
            }
            finally
            {
                // Ensure loading state is reset
                IsLoading = false;
            }
        }

        /// <summary>
        /// Refreshes the inbox by reloading messages
        /// Triggered manually by user pull-to-refresh or other refresh mechanisms
        /// </summary>
        [RelayCommand]
        private async Task RefreshMessagesAsync()
        {
            await LoadMessagesAsync();
        }

        /// <summary>
        /// Opens a specific message and marks it as read
        /// Navigates to message details page
        /// </summary>
        /// <param name="message">Selected message to open</param>
        [RelayCommand]
        private async Task OpenMessageAsync(Message message)
        {
            try
            {
                // Mark message as read before navigation
                await _messageService.MarkMessageAsReadAsync(message.Id);

                // Navigate to detailed message view
                // Assumes you have a route set up for MessageDetailPage
                await Shell.Current.GoToAsync($"MessageDetailPage?MessageId={message.Id}");
            }
            catch (Exception ex)
            {
                // Log any navigation or marking errors
                Debug.WriteLine($"Error opening message: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Could not open message", "OK");
            }
        }

        /// <summary>
        /// Deletes a specific message
        /// Provides user confirmation before deletion
        /// </summary>
        /// <param name="message">Message to be deleted</param>
        [RelayCommand]
        private async Task DeleteMessageAsync(Message message)
        {
            // Confirm message deletion with user
            bool confirm = await Shell.Current.DisplayAlert(
                "Delete Message",
                "Are you sure you want to delete this message?",
                "Delete",
                "Cancel");

            if (confirm)
            {
                try
                {
                    // Attempt to delete message
                    bool deleted = await _messageService.DeleteMessageAsync(message.Id);

                    if (deleted)
                    {
                        // Remove from local collection if successfully deleted
                        Messages.Remove(message);
                        Debug.WriteLine($"Message {message.Id} deleted successfully");
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("Error", "Could not delete message", "OK");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error deleting message: {ex.Message}");
                    await Shell.Current.DisplayAlert("Error", "An error occurred while deleting the message", "OK");
                }
            }
        }
    }
}