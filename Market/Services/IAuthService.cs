// Services/IAuthService.cs
using Market.DataAccess.Models;

namespace Market.Services
{
    public interface IAuthService
    {
        // Existing methods
        Task<bool> RegisterUserAsync(User user);
        Task<User?> SignInAsync(string email, string password);

        // New methods to resolve build errors
        /// <summary>
        /// Retrieves a user by their email address
        /// </summary>
        /// <param name="email">Email address of the user</param>
        /// <returns>User object if found, null otherwise</returns>
        Task<User?> GetUserByEmailAsync(string email);

        /// <summary>
        /// Changes a user's password
        /// </summary>
        /// <param name="email">User's email</param>
        /// <param name="currentPassword">Current password for verification</param>
        /// <param name="newPassword">New password to set</param>
        /// <returns>True if password changed successfully, false otherwise</returns>
        Task<bool> ChangePasswordAsync(string email, string currentPassword, string newPassword);
    }
}