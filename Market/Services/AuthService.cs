﻿using Market.DataAccess.Data;
using Market.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Market.Services
{
    /// <summary>
    /// Service handling user authentication operations including sign-in and registration
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private static SemaphoreSlim _initLock = new SemaphoreSlim(1, 1);
        private bool _isInitialized;

        // In AuthService.cs, add these methods:
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
                return null;

            await InitializeAsync();

            try
            {
                return await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving user by email: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> ChangePasswordAsync(string email, string currentPassword, string newPassword)
        {
            await InitializeAsync();

            try
            {
                // First, verify the current password
                var user = await SignInAsync(email, currentPassword);
                if (user == null)
                {
                    Debug.WriteLine("Current password verification failed");
                    return false;
                }

                // Hash the new password
                user.PasswordHash = PasswordHasher.HashPassword(newPassword);

                // Update the user
                _context.Users.Update(user);
                var result = await _context.SaveChangesAsync();

                Debug.WriteLine($"Password change result: {result}");
                return result > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Password change error: {ex.Message}");
                return false;
            }
        }

        public AuthService(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Initialize database and verify connection
        /// </summary>
        public async Task InitializeAsync()
        {
            if (_isInitialized) return;

            try
            {
                await _initLock.WaitAsync();
                if (_isInitialized) return;

                Debug.WriteLine("Starting database initialization");

                // Get and verify connection string
                var dbPath = (_context.Database.GetDbConnection()).ConnectionString;
                Debug.WriteLine($"Database connection string: {dbPath}");

                // Verify database exists and is accessible
                var exists = await _context.Database.CanConnectAsync();
                Debug.WriteLine($"Database exists and can connect: {exists}");

                if (!exists)
                {
                    Debug.WriteLine("Creating database...");
                    await _context.Database.EnsureCreatedAsync();
                    Debug.WriteLine("Database created successfully");
                }

                // Verify Users table
                var userCount = await _context.Users.CountAsync();
                Debug.WriteLine($"Current number of users in database: {userCount}");

                _isInitialized = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Database initialization error: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw;
            }
            finally
            {
                _initLock.Release();
            }
        }

        /// <summary>
        /// Register a new user in the system
        /// </summary>
        /// <param name="user">User object containing registration details</param>
        /// <returns>True if registration successful, false if user exists</returns>
        public async Task<bool> RegisterUserAsync(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            await InitializeAsync();

            try
            {
                Debug.WriteLine($"\nStarting registration for email: {user.Email}");

                // Check for existing user
                var duplicateUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == user.Email.ToLower());

                if (duplicateUser != null)
                {
                    Debug.WriteLine($"Found duplicate user with email: {user.Email}");
                    return false;
                }

                // Hash password and save user
                user.PasswordHash = PasswordHasher.HashPassword(user.PasswordHash);
                await _context.Users.AddAsync(user);
                var result = await _context.SaveChangesAsync();

                Debug.WriteLine($"Registration completed. Changes saved: {result}");
                return result > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Registration error: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        /// <summary>
        /// Authenticate a user with email and password
        /// </summary>
        /// <param name="email">User's email</param>
        /// <param name="password">User's password</param>
        /// <returns>User object if authentication successful, null otherwise</returns>
        public async Task<User?> SignInAsync(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Email and password are required");
            }

            await InitializeAsync();

            try
            {
                Debug.WriteLine($"\nAttempting sign in for email: {email}");

                // Find user by email
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

                if (user == null)
                {
                    Debug.WriteLine($"No user found with email: {email}");
                    return null;
                }

                // Verify password
                var isPasswordValid = PasswordHasher.VerifyPassword(password, user.PasswordHash);
                Debug.WriteLine($"Password verification result: {isPasswordValid}");

                return isPasswordValid ? user : null;
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
        public async Task<User?> GetCurrentUserAsync()
        {
            try
            {
                // Attempt to retrieve the stored user ID from secure storage
                string? storedUserId = await SecureStorage.GetAsync("userId");

                // Check if we have a stored user ID
                if (string.IsNullOrEmpty(storedUserId))
                {
                    Debug.WriteLine("No stored user ID found. User is not logged in.");
                    return null;
                }

                // Attempt to parse the stored user ID to an integer
                if (!int.TryParse(storedUserId, out int userId))
                {
                    Debug.WriteLine("Stored user ID is invalid. Clearing stored ID.");
                    await SecureStorage.SetAsync("userId", string.Empty);
                    return null;
                }

                // Attempt to retrieve the user from the database
                User? user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    Debug.WriteLine($"User with ID {userId} not found in database. Clearing stored ID.");
                    await SecureStorage.SetAsync("userId", string.Empty);
                    return null;
                }

                Debug.WriteLine($"Successfully retrieved current user. ID: {user.Id}, Email: {user.Email}");
                return user;
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur during the process
                Debug.WriteLine($"Error in GetCurrentUserAsync: {ex.Message}");
                return null;
            }
        }


        public async Task<int> GetCurrentUserIdAsync()
        {
            try
            {
                // Attempt to retrieve the stored user ID from secure storage
                string? storedUserId = await SecureStorage.GetAsync("userId");

                // Check if we have a stored user ID
                if (string.IsNullOrEmpty(storedUserId))
                {
                    Debug.WriteLine("No stored user ID found. User is not logged in.");
                    throw new InvalidOperationException("No user is currently logged in.");
                }

                // Attempt to parse the stored user ID to an integer
                if (!int.TryParse(storedUserId, out int userId))
                {
                    Debug.WriteLine("Stored user ID is invalid.");
                    throw new InvalidOperationException("Invalid stored user ID.");
                }

                // Optional: Additional verification by checking if user exists in database
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    Debug.WriteLine($"User with ID {userId} not found in database.");
                    throw new InvalidOperationException("Current user not found in database.");
                }

                Debug.WriteLine($"Successfully retrieved current user ID: {userId}");
                return userId;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetCurrentUserIdAsync: {ex.Message}");
                throw; // Re-throw to allow caller to handle the error
            }
        }
    }
}