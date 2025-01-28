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
        private readonly List<User> _users;

        public AuthService()
        {
            _users = new List<User>();  // Proper initialization
        }
        public Task<bool> RegisterUserAsync(User user)
        {
            try
            {
                if (_users.Any(u => u.Email == user.Email))
                {
                    return Task.FromResult(false);
                }

                _users.Add(user);
                return Task.FromResult(true);
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
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