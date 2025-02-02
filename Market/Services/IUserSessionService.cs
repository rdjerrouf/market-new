// Services/IUserSessionService.cs
using Market.DataAccess.Models;
namespace Market.Services
{
    /// <summary>
    /// Manages user session state and persistence
    /// </summary>
    public interface IUserSessionService
    {
        /// <summary>
        /// Currently logged-in user
        /// </summary>
        User? CurrentUser { get; }

        /// <summary>
        /// Checks if a user is currently logged in
        /// </summary>
        bool IsLoggedIn { get; }

        /// <summary>
        /// Sets the current user for the session
        /// </summary>
        /// <param name="user">User to set as current</param>
        void SetCurrentUser(User user);

        /// <summary>
        /// Clears the current user session
        /// </summary>
        void ClearCurrentUser();

        /// <summary>
        /// Saves the current user session
        /// </summary>
        /// <returns>Task representing the save operation</returns>
        Task SaveSessionAsync();

        /// <summary>
        /// Attempts to restore a previous user session
        /// </summary>
        /// <returns>True if session was restored, false otherwise</returns>
        Task<bool> RestoreSessionAsync();
    }
}