// Services/AuthService.cs
using Market.Models;

namespace Market.Services
{
    public class AuthService : IAuthService
    {
        // In-memory user storage (temporary solution)
        private readonly List<User> _users = new();

        // Handle user registration
        public async Task<bool> RegisterUserAsync(User user)
        {
            // Use Task.Run since this is a CPU-bound operation
            return await Task.Run(() =>
            {
                try
                {
                    // Check for existing email
                    if (_users.Any(u => u.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase)))
                        return false;

                    // Add user to storage
                    _users.Add(user);
                    return true;
                }
                catch
                {
                    // Return false on any errors
                    return false;
                }
            });
        }

        // Handle user sign in
        public async Task<User?> SignInAsync(string email, string password)
        {
            // Use Task.Run since this is a CPU-bound operation
            return await Task.Run(() =>
            {
                // Find and return user with matching credentials
                return _users.FirstOrDefault(u =>
                    u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            });
        }
    }
}