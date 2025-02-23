using System;
using System.Threading.Tasks;
using Market.DataAccess.Data;
using Market.Market.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Market.Services
{
    public class VerificationService : IVerificationService
    {
        private readonly AppDbContext _context;

        public VerificationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateVerificationTokenAsync(int userId, VerificationType type)
        {
            try
            {
                // Generate a cryptographically strong random token
                var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray())[..20]
                    .Replace("/", "")
                    .Replace("+", "");

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    throw new InvalidOperationException($"User with ID {userId} not found");

                var verificationToken = new VerificationToken
                {
                    UserId = userId,
                    User = user,
                    Token = token,
                    Type = type,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(24),
                    IsUsed = false
                };

                await _context.VerificationTokens.AddAsync(verificationToken);
                await _context.SaveChangesAsync();

                return token;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error generating verification token: {ex.Message}");
                throw; // Re-throw the exception instead of returning null
            }
        }
        public async Task<bool> ValidateVerificationTokenAsync(string token, VerificationType type)
        {
            try
            {
                var verificationToken = await _context.VerificationTokens
                    .FirstOrDefaultAsync(vt =>
                        vt.Token == token &&
                        vt.Type == type &&
                        vt.ExpiresAt > DateTime.UtcNow &&
                        !vt.IsUsed);

                return verificationToken != null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error validating verification token: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> MarkTokenAsUsedAsync(string token)
        {
            try
            {
                var verificationToken = await _context.VerificationTokens
                    .FirstOrDefaultAsync(vt => vt.Token == token);

                if (verificationToken == null)
                    return false;

                verificationToken.IsUsed = true;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error marking token as used: {ex.Message}");
                return false;
            }
        }
    }
}