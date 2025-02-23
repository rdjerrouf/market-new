// IEmailService.cs in the Services folder
using System.Threading.Tasks;

namespace Market.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailVerificationAsync(string toEmail, string verificationLink);
        Task<bool> SendPasswordResetAsync(string toEmail, string resetLink);
        Task<bool> SendGenericEmailAsync(string toEmail, string subject, string body);
    }
}