// Services/PasswordHasher.cs
using System.Security.Cryptography;
using System.Text;

namespace Market.Services
{
    /// <summary>
    /// Handles password hashing operations
    /// Note: This is a basic implementation. In production, use a proper hashing library like BCrypt
    /// </summary>
    public static class PasswordHasher
    {
        /// <summary>
        /// Creates a hash from a password
        /// </summary>
        /// <param name="password">The password to hash</param>
        /// <returns>The hashed password</returns>
        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        /// <summary>
        /// Verifies a password against a hash
        /// </summary>
        /// <param name="password">The password to verify</param>
        /// <param name="hashedPassword">The hash to verify against</param>
        /// <returns>True if the password matches the hash, false otherwise</returns>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == hashedPassword;
        }
    }
}