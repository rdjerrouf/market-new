// Services/AuthService.cs
using Market.DataAccess.Data;
using Market.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
namespace Market.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;

        public AuthService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> RegisterUserAsync(User user)
        {
            try
            {
                // Modify this line to use string comparison
                var existingUser = await _context.Users.FirstOrDefaultAsync(u =>
                    u.Email.ToLower() == user.Email.ToLower());

                if (existingUser != null)
                    return false;

                // Hash the password
                user.PasswordHash = PasswordHasher.HashPassword(user.PasswordHash);

                // Add and save
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Registration error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<User?> SignInAsync(string email, string password)
        {
            // Find user with matching email
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

            if (user == null)
                return null;

            // Verify password (you'll need to add password verification to PasswordHasher)
            if (!PasswordHasher.VerifyPassword(password, user.PasswordHash))
                return null;

            return user;
        }
    }
}