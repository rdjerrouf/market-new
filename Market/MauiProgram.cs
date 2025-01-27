using Microsoft.Extensions.Logging;
using Market.Services;
using Market.ViewModels;
using Market.Views;
using Market.Models;

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
            builder.Services.AddTransient<SignInViewModel>();
            builder.Services.AddTransient<RegistrationViewModel>();

            return builder.Build();
        }
    }
}