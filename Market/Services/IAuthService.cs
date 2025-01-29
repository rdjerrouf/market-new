using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Market.DataAccess.Models;

namespace Market.Services
{
    public interface IAuthService
    {
        Task<bool> RegisterUserAsync(User user);
        Task<User?> SignInAsync(string email, string password);
    }
}