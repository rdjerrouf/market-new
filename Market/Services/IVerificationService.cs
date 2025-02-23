using System.Threading.Tasks;
using Market.Market.DataAccess.Models;

namespace Market.Services
{
    public interface IVerificationService
    {
        Task<string> GenerateVerificationTokenAsync(int userId, VerificationType type);
        Task<bool> ValidateVerificationTokenAsync(string token, VerificationType type);
        Task<bool> MarkTokenAsUsedAsync(string token);
    }
}