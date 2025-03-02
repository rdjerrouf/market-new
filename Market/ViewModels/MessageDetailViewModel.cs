using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Market.DataAccess.Models;
using Market.Views;
using Market.Services;
using System.Diagnostics;

namespace Market.ViewModels
{
    public partial class MessageDetailViewModel : ObservableObject
    {
        private readonly IMessageService _messageService;
        private readonly IAuthService _authService;
        private readonly IItemService _itemService;

        [ObservableProperty]
        private Message _message;

        [ObservableProperty]
        private string _replyText = string.Empty;

        [ObservableProperty]
        private Item _relatedItem;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private bool _hasRelatedItem;

        [ObservableProperty]
        private bool _isOwnMessage;

        [ObservableProperty]
        private string _senderName = string.Empty;

        public MessageDetailViewModel(IMessageService messageService, IAuthService authService, IItemService itemService)
        {
            _messageService = messageService;
            _authService = authService;
            _itemService = itemService;

            // In the constructor, initialize Message with required properties:
            Message = new Message
            {
                Content = string.Empty,
                SenderId = 0,
                ReceiverId = 0
            };
     //       Message = new Message();
        }

        public async Task InitializeAsync(int messageId)
        {
            try
            {
                IsBusy = true;
                Debug.WriteLine($"Loading message details for ID: {messageId}");

                // Load the message
                var message = await _messageService.GetMessageByIdAsync(messageId);
                if (message == null)
                {
                    StatusMessage = "Message not found";
                    Debug.WriteLine("Message not found");
                    return;
                }

                Message = message;
                Debug.WriteLine($"Message loaded: {message.Content}");

                // Mark the message as read if it isn't already
                if (!message.IsRead)
                {
                    Debug.WriteLine("Marking message as read");
                    await _messageService.MarkMessageAsReadAsync(messageId);
                    message.IsRead = true;
                }

                // Get the current user to determine if this is our message
                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser != null)
                {
                    IsOwnMessage = message.SenderId == currentUser.Id;
                    Debug.WriteLine($"Is own message: {IsOwnMessage}");

                    // Get sender name if it's not our message
                    if (!IsOwnMessage)
                    {
                        var sender = await _authService.GetUserByEmailAsync("user@example.com"); // Temporary workaround
                        SenderName = sender?.DisplayName ?? $"User {message.SenderId}";
                        Debug.WriteLine($"Sender name: {SenderName}");
                    }
                }

                // If there's a related item, load it
                if (message.RelatedItemId.HasValue)
                {
                    Debug.WriteLine($"Loading related item: {message.RelatedItemId}");
                    var item = await _itemService.GetItemAsync(message.RelatedItemId.Value);
                    if (item != null)
                    {
                        RelatedItem = item;
                        HasRelatedItem = true;
                        Debug.WriteLine($"Related item loaded: {item.Title}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading message: {ex.Message}");
                StatusMessage = "Failed to load message details";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task SendReply()
        {
            if (IsBusy || string.IsNullOrWhiteSpace(ReplyText))
                return;

            try
            {
                IsBusy = true;
                Debug.WriteLine("Sending reply");

                // Temporary hardcoded user ID - replace with actual auth logic
                int currentUserId = 2; // Replace with actual user ID from auth service

                // Create the reply message
                var reply = new Message
                {
                    Content = ReplyText,
                    SenderId = currentUserId,
                    ReceiverId = IsOwnMessage ? Message.ReceiverId : Message.SenderId,
                    RelatedItemId = Message.RelatedItemId,
                    Timestamp = DateTime.UtcNow,
                    IsRead = false
                };

                Debug.WriteLine($"Reply created to: {reply.ReceiverId}");

                // Send the message
                bool success = await _messageService.SendMessageAsync(reply);

                if (success)
                {
                    Debug.WriteLine("Reply sent successfully");
                    StatusMessage = "Reply sent successfully";
                    ReplyText = string.Empty;

                    // Wait a moment to show success message
                    await Task.Delay(1000);

                    // Navigate back to inbox
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    Debug.WriteLine("Failed to send reply");
                    StatusMessage = "Failed to send reply";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error sending reply: {ex.Message}");
                StatusMessage = "Error sending reply";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task DeleteMessage()
        {
            if (IsBusy)
                return;

            try
            {
                Debug.WriteLine("Requesting message deletion confirmation");
                bool confirm = await Shell.Current.DisplayAlert(
                    "Delete Message",
                    "Are you sure you want to delete this message?",
                    "Delete",
                    "Cancel");

                if (!confirm)
                {
                    Debug.WriteLine("Message deletion cancelled");
                    return;
                }

                IsBusy = true;
                Debug.WriteLine($"Deleting message: {Message.Id}");

                bool success = await _messageService.DeleteMessageAsync(Message.Id);

                if (success)
                {
                    Debug.WriteLine("Message deleted successfully");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    Debug.WriteLine("Failed to delete message");
                    StatusMessage = "Failed to delete message";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting message: {ex.Message}");
                StatusMessage = "Error deleting message";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task ViewRelatedItem()
        {
            if (HasRelatedItem && RelatedItem != null)
            {
                Debug.WriteLine($"Navigating to related item: {RelatedItem.Id}");
                await Shell.Current.GoToAsync($"{nameof(ItemDetailPage)}?ItemId={RelatedItem.Id}");
            }
        }
    }
}