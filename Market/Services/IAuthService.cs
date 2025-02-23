using Market.DataAccess.Models;
using Market.Market.DataAccess.Models.Dtos;

namespace Market.Services
{
    public interface IAuthService
    {
        Task<bool> RegisterUserAsync(User user);
        Task<User?> SignInAsync(string email, string password);
        Task<User?> GetUserByEmailAsync(string email);
        Task<bool> ChangePasswordAsync(string email, string currentPassword, string newPassword);
        Task<bool> ResetPasswordAsync(string email, string newPassword);
        Task<bool> IsEmailRegisteredAsync(string email);
        Task<User?> GetCurrentUserAsync();
        Task<int> GetCurrentUserIdAsync();
        Task<bool> UpdateUserProfileAsync(int userId, string displayName, string profilePicture, string bio);
        Task<bool> UpdateUserPrivacyAsync(int userId, bool showEmail, bool showPhoneNumber);
        Task<bool> UpdateUserContactInfoAsync(int userId, string? phoneNumber, string? city, string? province);
        Task<UserProfileDto?> GetUserProfileAsync(int userId);
        Task<bool> SendEmailVerificationTokenAsync(int userId);
        Task<bool> VerifyEmailAsync(string token);
        Task<bool> IsEmailVerifiedAsync(int userId);
        Task<bool> ConfirmEmailAsync(string userId, string token);
        Task<string> GenerateEmailVerificationTokenAsync(User user);
        Task InitializeAsync();
    }
}