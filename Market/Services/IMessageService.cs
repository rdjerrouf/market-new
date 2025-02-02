// Services/IMessageService.cs
using Market.DataAccess.Models;

namespace Market.Services
{
    /// <summary>
    /// Defines contract for message-related operations in the marketplace
    /// </summary>
    public interface IMessageService
    {
        /// <summary>
        /// Retrieves all messages for a specific user
        /// </summary>
        /// <param name="userId">ID of the user whose messages are to be retrieved</param>
        /// <returns>Collection of messages for the user</returns>
        Task<IEnumerable<Message>> GetUserInboxMessagesAsync(int userId);

        /// <summary>
        /// Sends a new message
        /// </summary>
        /// <param name="message">Message to be sent</param>
        /// <returns>True if message sent successfully, false otherwise</returns>
        Task<bool> SendMessageAsync(Message message);

        /// <summary>
        /// Marks a specific message as read
        /// </summary>
        /// <param name="messageId">ID of the message to mark as read</param>
        /// <returns>True if message status updated successfully, false otherwise</returns>
        Task<bool> MarkMessageAsReadAsync(int messageId);

        /// <summary>
        /// Deletes a specific message
        /// </summary>
        /// <param name="messageId">ID of the message to delete</param>
        /// <returns>True if message deleted successfully, false otherwise</returns>
        Task<bool> DeleteMessageAsync(int messageId);

        /// <summary>
        /// Retrieves a specific message by its ID
        /// </summary>
        /// <param name="messageId">ID of the message to retrieve</param>
        /// <returns>Message if found, null otherwise</returns>
        Task<Message?> GetMessageByIdAsync(int messageId);
    }
}