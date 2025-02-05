using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
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
        public InboxViewModel(IMessageService messageService, IAuthService authService)
        {
            Debug.WriteLine("InboxViewModel constructor called");
            Console.WriteLine("InboxViewModel constructor called");

            _messageService = messageService;
            _authService = authService;

            Debug.WriteLine("Executing LoadMessagesCommand");
            Console.WriteLine("Executing LoadMessagesCommand");
            LoadMessagesCommand.Execute(null);

            Debug.WriteLine("InboxViewModel constructor completed");
            Console.WriteLine("InboxViewModel constructor completed");
        }
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
        }        
                 /// Constructor for InboxViewModel
        [RelayCommand]
        private async Task LoadMessagesAsync()
        {
            Debug.WriteLine("LoadMessagesAsync called");
            if (IsLoading) return;
            try
            {
                IsLoading = true;
                Debug.WriteLine("InboxViewModel: Set IsLoading to true");
                int currentUserId = 2; // TODO: Replace with actual user ID
                Debug.WriteLine($"Fetching messages for user ID: {currentUserId}");
                var messages = await _messageService.GetUserInboxMessagesAsync(currentUserId);
                Debug.WriteLine($"InboxViewModel: Retrieved {messages.Count()} messages from service");
                Messages.Clear();
                foreach (var message in messages)
                {
                    Messages.Add(message);
                    Debug.WriteLine($"InboxViewModel: Added message: ID={message.Id}, Content={message.Content}");
                }
                Debug.WriteLine($"InboxViewModel: Added messagee: ID= {Messages.Count}");
                foreach (var message in Messages)
                {
                    Debug.WriteLine($"Message: {message.Content}, From: {message.SenderId}, Time: {message.Timestamp}");
                }
                Debug.WriteLine($"InboxViewModel: Total messages loaded: {Messages.Count}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading messages: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Unable to load messages", "OK");
            }
            finally
            {
                IsLoading = false;
            }
            Debug.WriteLine("InboxViewModel: LoadMessagesAsync completed");
        }

        /// <summary>
        /// Refreshes the inbox by reloading messages
        /// Triggered manually by user pull-to-refresh or other refresh mechanisms
        /// </summary>
        [RelayCommand]
        private Task RefreshMessagesAsync() => LoadMessagesAsync();

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