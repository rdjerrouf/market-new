// Services/PasswordHasher.cs
using System.Security.Cryptography;

namespace Market.Services
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int HashSize = 20;

        public static string HashPassword(string password)
        {
            // Use modern RandomNumberGenerator instead of RNGCryptoServiceProvider
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = GetHash(password, salt);

            byte[] hashBytes = new byte[SaltSize + HashSize];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

            return Convert.ToBase64String(hashBytes);
        }

        private static byte[] GetHash(string password, byte[] salt)
        {
            // Use modern constructor with SHA256 and proper iteration count
            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                100000, // Increased iterations for security
                HashAlgorithmName.SHA256);

            return pbkdf2.GetBytes(HashSize);
        }
    }
}