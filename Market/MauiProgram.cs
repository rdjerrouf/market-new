using Microsoft.Extensions.Logging;
using Market.Services;
using Market.ViewModels;
using Market.Views;
using Market.DataAccess.Models;
using Market.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Market
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            // Configure SQLite database
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "market.db");
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            // Register Services
            builder.Services.AddSingleton<IAuthService, AuthService>();

            // Register ViewModels
            builder.Services.AddTransient<SignInViewModel>();
            builder.Services.AddTransient<RegistrationViewModel>();
            builder.Services.AddTransient<MainViewModel>();
            builder.Services.AddTransient<SearchViewModel>();

            // Register Pages
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<SignInPage>();
            builder.Services.AddTransient<RegistrationPage>();
            builder.Services.AddTransient<SearchPage>();

            return builder.Build();
        }
    }
}