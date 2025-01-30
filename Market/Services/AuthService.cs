using Market.DataAccess.Data;
using Market.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Market.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;

        public AuthService(AppDbContext context)
        {
            _context = context;
            InitializeDatabase().Wait();
        }

        private async Task InitializeDatabase()
        {
            try
            {
                Debug.WriteLine("Starting database initialization");

                // Get database path
                var dbPath = (_context.Database.GetDbConnection()).ConnectionString;
                Debug.WriteLine($"Database connection string: {dbPath}");

                // Check if database exists
                var exists = await _context.Database.CanConnectAsync();
                Debug.WriteLine($"Database exists and can connect: {exists}");

                if (!exists)
                {
                    Debug.WriteLine("Creating database...");
                    await _context.Database.EnsureCreatedAsync();
                    Debug.WriteLine("Database created successfully");
                }

                // Verify Users table by attempting to count records
                try
                {
                    var userCount = await _context.Users.CountAsync();
                    Debug.WriteLine($"Current number of users in database: {userCount}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error accessing Users table: {ex.Message}");
                    throw new Exception("Users table not properly initialized");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Database initialization error: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw; // Re-throw to ensure app doesn't start with broken DB
            }
        }

        public async Task<bool> RegisterUserAsync(User user)
        {
            try
            {
                Debug.WriteLine($"\nStarting registration for email: {user.Email}");

                // Verify database connection
                if (!await _context.Database.CanConnectAsync())
                {
                    Debug.WriteLine("Cannot connect to database!");
                    throw new Exception("Database connection failed");
                }

                // Check for existing user
                var duplicateUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == user.Email.ToLower());

                if (duplicateUser != null)
                {
                    Debug.WriteLine($"Found duplicate user with email: {duplicateUser.Email}");
                    return false;
                }

                Debug.WriteLine("No duplicate user found, proceeding with registration");

                // Hash password
                user.PasswordHash = PasswordHasher.HashPassword(user.PasswordHash);
                Debug.WriteLine("Password hashed successfully");

                // Add user to context
                await _context.Users.AddAsync(user);
                Debug.WriteLine("User added to context, trying to save...");

                // Try to save
                try
                {
                    var result = await _context.SaveChangesAsync();
                    Debug.WriteLine($"SaveChanges completed. Changes saved: {result}");
                    return result > 0;
                }
                catch (DbUpdateException dbEx)
                {
                    Debug.WriteLine($"Database update error: {dbEx.Message}");
                    if (dbEx.InnerException != null)
                    {
                        Debug.WriteLine($"Inner exception: {dbEx.InnerException.Message}");
                    }
                    throw;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Registration error: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw; // Re-throw so we can see the actual error in the UI
            }
        }

        public async Task<User?> SignInAsync(string email, string password)
        {
            try
            {
                Debug.WriteLine($"\nAttempting sign in for email: {email}");

                if (!await _context.Database.CanConnectAsync())
                {
                    Debug.WriteLine("Cannot connect to database during sign in!");
                    throw new Exception("Database connection failed");
                }

                // Log total number of users in database
                var userCount = await _context.Users.CountAsync();
                Debug.WriteLine($"Total users in database: {userCount}");

                // Log all users in database for debugging
                var allUsers = await _context.Users.ToListAsync();
                Debug.WriteLine("All users in database:");
                foreach (var u in allUsers)
                {
                    Debug.WriteLine($"- Email: {u.Email}, Id: {u.Id}");
                }

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

                if (user == null)
                {
                    Debug.WriteLine($"No user found with email: {email}");
                    return null;
                }

                Debug.WriteLine($"User found - Id: {user.Id}, Email: {user.Email}");
                Debug.WriteLine("Verifying password");

                var isPasswordValid = PasswordHasher.VerifyPassword(password, user.PasswordHash);
                Debug.WriteLine($"Password verification result: {isPasswordValid}");

                if (!isPasswordValid)
                {
                    Debug.WriteLine("Password verification failed");
                    return null;
                }

                Debug.WriteLine("Sign in successful");
                return user;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Sign in error: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }
    }
}