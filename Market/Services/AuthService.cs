// Services/AuthService.cs
using Market.Models;
using System.Text.RegularExpressions;

namespace Market.Services
{
    /// <summary>
    /// Service handling user authentication operations including registration and sign-in
    /// </summary>
    public class AuthService : IAuthService
    {
        // Temporary in-memory storage - will be replaced with database
        private readonly List<User> _users = new();

        /// <summary>
        /// Registers a new user in the system
        /// </summary>
        /// <param name="user">User object containing registration information</param>
        /// <returns>True if registration successful, false otherwise</returns>
        public Task<bool> RegisterUserAsync(User user)
        {
            // Check if email is already registered
            if (_users.Any(u => u.Email == user.Email))
            {
                return Task.FromResult(false);
            }

            // Validate password complexity
            // Note: In production, this would validate the raw password before hashing
            if (!IsValidPassword(user.PasswordHash))
            {
                return Task.FromResult(false);
            }

            // Add user to storage
            _users.Add(user);
            return Task.FromResult(true);
        }

        /// <summary>
        /// Authenticates a user based on email and password
        /// </summary>
        /// <param name="email">User's email address</param>
        /// <param name="passwordHash">Hashed password</param>
        /// <returns>User object if authentication successful, null otherwise</returns>
        public Task<User?> SignInAsync(string email, string passwordHash)
        {
            // Find user with matching credentials
            // Note: In production, would hash input password before comparing
            return Task.FromResult(_users.FirstOrDefault(u =>
                u.Email == email && u.PasswordHash == passwordHash));
        }

        /// <summary>
        /// Validates password complexity requirements
        /// </summary>
        /// <param name="password">Password to validate</param>
        /// <returns>True if password meets requirements, false otherwise</returns>
        private bool IsValidPassword(string password)
        {
            // Password must contain:
            // - At least 8 characters
            // - At least one lowercase letter
            // - At least one uppercase letter
            // - At least one digit
            // - At least one special character
            var passwordRegex = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\w\d]).{8,}$";
            return Regex.IsMatch(password, passwordRegex);
        }
    }
}