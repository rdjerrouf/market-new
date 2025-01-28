using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Helpers/InputValidator.cs
namespace Market.Helpers
{
    public static class InputValidator
    {
        // Basic email validation
        public static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        // Check password meets requirements
        public static bool IsValidPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return false;

            return password.Length >= 8 &&
                   password.Any(char.IsUpper) &&
                   password.Any(char.IsLower) &&
                   password.Any(char.IsDigit);
        }
    }
}