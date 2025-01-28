using Microsoft.Extensions.Logging;
using Market.Services;
using Market.ViewModels;
using Market.Views;
using Market.Models;
using Market.Data;

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

            // Register services
            builder.Services.AddSingleton<IAuthService, AuthService>();

            // Register ViewModels
            builder.Services.AddTransient<SignInViewModel>();
            builder.Services.AddTransient<RegistrationViewModel>();
            builder.Services.AddTransient<MainViewModel>();

            // Register Pages
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<SignInPage>();
            builder.Services.AddTransient<RegistrationPage>();

            return builder.Build();
        }
    }
}