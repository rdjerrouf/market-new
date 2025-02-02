// Services/UserSessionService.cs
using System.Diagnostics;
using Market.DataAccess.Models;
using Market.Services;

namespace Market.Services
{
    /// <summary>
    /// Implements user session management using secure storage
    /// </summary>
    public class UserSessionService : IUserSessionService
    {
        private readonly ISecureStorage _secureStorage;
        private readonly IAuthService _authService;

        /// <summary>
        /// Current user in the session
        /// </summary>
        public User? CurrentUser { get; private set; }

        /// <summary>
        /// Checks if a user is currently logged in
        /// </summary>
        public bool IsLoggedIn => CurrentUser != null;

        /// <summary>
        /// Constructor for UserSessionService
        /// </summary>
        /// <param name="secureStorage">Secure storage for persisting session</param>
        /// <param name="authService">Authentication service for user retrieval</param>
        public UserSessionService(ISecureStorage secureStorage, IAuthService authService)
        {
            _secureStorage = secureStorage;
            _authService = authService;
        }

        /// <summary>
        /// Sets the current user for the session
        /// </summary>
        public void SetCurrentUser(User user)
        {
            CurrentUser = user;
        }

        /// <summary>
        /// Clears the current user session
        /// </summary>
        public void ClearCurrentUser()
        {
            CurrentUser = null;
            _secureStorage.Remove("user_email");
        }

        /// <summary>
        /// Saves the current user session
        /// </summary>
        public async Task SaveSessionAsync()
        {
            if (CurrentUser != null)
            {
                try
                {
                    await _secureStorage.SetAsync("user_email", CurrentUser.Email);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error saving session: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Attempts to restore a previous user session
        /// </summary>
        public async Task<bool> RestoreSessionAsync()
        {
            try
            {
                var savedEmail = await _secureStorage.GetAsync("user_email");

                if (string.IsNullOrEmpty(savedEmail))
                    return false;

                // Retrieve user by email
                var user = await _authService.GetUserByEmailAsync(savedEmail);

                if (user != null)
                {
                    CurrentUser = user;
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Session restore error: {ex.Message}");
            }

            return false;
        }
    }
}