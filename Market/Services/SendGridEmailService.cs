// SendGridEmailService.cs in the Services folder
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Diagnostics;
using System.Net.Mail;

namespace Market.Services
{
    public class SendGridEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly string? _sendGridApiKey;

        public SendGridEmailService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _sendGridApiKey = _configuration["SendGrid:ApiKey"];

            if (string.IsNullOrEmpty(_sendGridApiKey))
            {
                throw new InvalidOperationException("SendGrid API key not found in configuration");
            }
        }

        public async Task<bool> SendEmailVerificationAsync(string toEmail, string verificationLink)
        {
            try
            {
                var client = new SendGridClient(_sendGridApiKey);
                var from = new EmailAddress(_configuration["SendGrid:FromEmail"], "Market App");
                var subject = "Verify Your Email";
                var to = new EmailAddress(toEmail);
                var htmlContent = $@"
                    <h1>Email Verification</h1>
                    <p>Click the link below to verify your email:</p>
                    <a href='{verificationLink}'>Verify Email</a>
                    <p>If you did not create an account, please ignore this email.</p>
                ";

                var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);
                var response = await client.SendEmailAsync(msg);

                Debug.WriteLine($"Email sent to {toEmail}. Status: {response.StatusCode}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error sending verification email: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendPasswordResetAsync(string toEmail, string resetLink)
        {
            try
            {
                var client = new SendGridClient(_sendGridApiKey);
                var from = new EmailAddress(_configuration["SendGrid:FromEmail"], "Market App");
                var subject = "Password Reset";
                var to = new EmailAddress(toEmail);
                var htmlContent = $@"
                    <h1>Password Reset</h1>
                    <p>Click the link below to reset your password:</p>
                    <a href='{resetLink}'>Reset Password</a>
                    <p>If you did not request a password reset, please ignore this email.</p>
                ";

                var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);
                var response = await client.SendEmailAsync(msg);

                Debug.WriteLine($"Password reset email sent to {toEmail}. Status: {response.StatusCode}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error sending password reset email: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendGenericEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var client = new SendGridClient(_sendGridApiKey);
                var from = new EmailAddress(_configuration["SendGrid:FromEmail"], "Market App");
                var to = new EmailAddress(toEmail);

                var msg = MailHelper.CreateSingleEmail(from, to, subject, null, body);
                var response = await client.SendEmailAsync(msg);

                Debug.WriteLine($"Generic email sent to {toEmail}. Status: {response.StatusCode}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error sending generic email: {ex.Message}");
                return false;
            }
        }
    }
}