// Services/MessageService.cs
using Market.DataAccess.Data;
using Market.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Market.Services
{
    /// <summary>
    /// Implements message-related operations using Entity Framework Core
    /// </summary>
    public class MessageService : IMessageService
    {
        /// <summary>
        /// Database context for data access
        /// </summary>
        private readonly AppDbContext _context;
        private readonly IItemService _itemService;
        /// <summary>
        /// Constructor that injects the database context
        /// </summary>
        /// <param name="context">Database context for message operations</param>
        public MessageService(AppDbContext context, IItemService itemService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _itemService = itemService ?? throw new ArgumentNullException(nameof(itemService));
        }

        /// <summary>
        /// Retrieves all messages for a specific user
        /// </summary>
        // Services/MessageService.cs
        public async Task<IEnumerable<Message>> GetUserInboxMessagesAsync(int userId)
        {
            try
            {
                Debug.WriteLine($"GetUserInboxMessagesAsync called for user {userId}");
                Debug.WriteLine($"Database path: {_context.Database.GetDbConnection().ConnectionString}");

                var query = _context.Messages.Where(m => m.ReceiverId == userId);
                Debug.WriteLine($"Query SQL: {query.ToQueryString()}");

                var messages = await query.OrderByDescending(m => m.Timestamp).ToListAsync();

                Debug.WriteLine($"Retrieved {messages.Count} messages for user {userId}");
                foreach (var message in messages)
                {
                    Debug.WriteLine($"Message: ID={message.Id}, Content={message.Content}, From={message.SenderId}, To={message.ReceiverId}, Time={message.Timestamp}");
                }

                if (messages.Count == 0)
                {
                    var allMessages = await _context.Messages.ToListAsync();
                    Debug.WriteLine($"Total messages in database: {allMessages.Count}");
                    foreach (var msg in allMessages)
                    {
                        Debug.WriteLine($"DB Message: ID={msg.Id}, From={msg.SenderId}, To={msg.ReceiverId}");
                    }
                }

                return messages;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving user inbox messages: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return Enumerable.Empty<Message>();
            }
        }
        /// <summary>
        /// Sends a new message
        /// </summary>
        public async Task<bool> SendMessageAsync(Message message)
        {
            try
            {
                // Ensure timestamp is set to current UTC time
                message.Timestamp = DateTime.UtcNow;

                // Mark as unread by default
                message.IsRead = false;

                // If the message is related to an item, record an inquiry
                if (message.RelatedItemId.HasValue)
                {
                    await _itemService.RecordItemInquiryAsync(message.RelatedItemId.Value);
                }

                // Add message to database
                await _context.Messages.AddAsync(message);
                int result = await _context.SaveChangesAsync();

                return result > 0;
            }
            catch (Exception ex)
            {
                // Log sending errors
                Debug.WriteLine($"Error sending message: {ex.Message}");
                return false;
            }
        }
        /// <summary>
        /// Marks a specific message as read
        /// </summary>
        public async Task<bool> MarkMessageAsReadAsync(int messageId)
        {
            try
            {
                // Find the message
                var message = await _context.Messages.FindAsync(messageId);

                if (message == null)
                {
                    Debug.WriteLine($"Message with ID {messageId} not found");
                    return false;
                }

                // Mark as read
                message.IsRead = true;
                int result = await _context.SaveChangesAsync();

                return result > 0;
            }
            catch (Exception ex)
            {
                // Log marking errors
                Debug.WriteLine($"Error marking message as read: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Deletes a specific message
        /// </summary>
        public async Task<bool> DeleteMessageAsync(int messageId)
        {
            try
            {
                // Find the message
                var message = await _context.Messages.FindAsync(messageId);

                if (message == null)
                {
                    Debug.WriteLine($"Message with ID {messageId} not found");
                    return false;
                }

                // Remove message from database
                _context.Messages.Remove(message);
                int result = await _context.SaveChangesAsync();

                return result > 0;
            }
            catch (Exception ex)
            {
                // Log deletion errors
                Debug.WriteLine($"Error deleting message: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Retrieves a specific message by its ID
        /// </summary>
        public async Task<Message?> GetMessageByIdAsync(int messageId)
        {
            try
            {
                return await _context.Messages.FindAsync(messageId);
            }
            catch (Exception ex)
            {
                // Log retrieval errors
                Debug.WriteLine($"Error retrieving message: {ex.Message}");
                return null;
            }
        }
    }
}