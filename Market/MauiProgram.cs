using Microsoft.Extensions.Logging;
using Market.Services;
using Market.ViewModels;
using Market.Views;
using Market.DataAccess.Models;
using Market.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Market
{
    // Static class responsible for MAUI application setup and configuration
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            Debug.WriteLine("Starting application initialization...");

            var builder = MauiApp.CreateBuilder();

            // Configure basic MAUI settings
            ConfigureBasicSettings(builder);

            // Set up database
            ConfigureDatabase(builder);

            // Register services, viewmodels, and pages
            RegisterServices(builder);
            RegisterViewModels(builder);
            RegisterPages(builder);

            // Configure debug settings
            ConfigureDebugSettings(builder);

            // Build the application
            var app = builder.Build();

            // Initialize database
            InitializeDatabase(app);

            Debug.WriteLine("Application initialization completed");
            return app;
        }

        // Configures basic MAUI application settings
        private static void ConfigureBasicSettings(MauiAppBuilder builder)
        {
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });
        }

        // Configures database settings and connection
        private static void ConfigureDatabase(MauiAppBuilder builder)
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "market.db");
            Debug.WriteLine($"Setting up database at: {dbPath}");

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite($"Data Source={dbPath}");
#if DEBUG
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
                options.LogTo(message => Debug.WriteLine($"EF Core: {message}"));
#endif
            }, ServiceLifetime.Scoped);
        }

        // Registers application services with dependency injection
        private static void RegisterServices(MauiAppBuilder builder)
        {
            Debug.WriteLine("Registering services...");

            // Register AuthService
            builder.Services.AddScoped<IAuthService>(provider => {
                var context = provider.GetRequiredService<AppDbContext>();
                var service = new AuthService(context);
                service.InitializeAsync().GetAwaiter().GetResult();
                return service;
            });

            // Add ItemService registration
            builder.Services.AddScoped<IItemService, ItemService>();
            builder.Services.AddScoped<IMessageService, MessageService>();
            // Other service registrations...
        }        // Registers view models with dependency injection
        private static void RegisterViewModels(MauiAppBuilder builder)
        {
            Debug.WriteLine("Registering ViewModels...");

            builder.Services.AddTransient<SignInViewModel>();
            builder.Services.AddTransient<RegistrationViewModel>();
            builder.Services.AddTransient<MainViewModel>();
            builder.Services.AddTransient<SearchViewModel>();
            builder.Services.AddTransient<PostItemViewModel>();
            builder.Services.AddTransient<InboxViewModel>();
            builder.Services.AddTransient<MyListingsViewModel>();
        }

        // Registers pages with dependency injection
        private static void RegisterPages(MauiAppBuilder builder)
        {
            Debug.WriteLine("Registering Pages...");

            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<SignInPage>();
            builder.Services.AddTransient<RegistrationPage>();
            builder.Services.AddTransient<SearchPage>();
            builder.Services.AddTransient<PostItemPage>();
            builder.Services.AddTransient<InboxPage>();
            builder.Services.AddTransient<MyListingsPage>();
        }

        // Configures debug settings for development
        private static void ConfigureDebugSettings(MauiAppBuilder builder)
        {
#if DEBUG
            builder.Logging.AddDebug().SetMinimumLevel(LogLevel.Debug);
#endif
        }

        // Initializes the database with required schema
        private static void InitializeDatabase(MauiApp app)
        {
            Debug.WriteLine("Initializing database...");

            using var scope = app.Services.CreateScope();
            try
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var dbPath = Path.Combine(FileSystem.AppDataDirectory, "market.db");

                if (!File.Exists(dbPath))
                {
                    context.Database.EnsureCreated();
                    Debug.WriteLine("Database created successfully");
                }
                else if (context.Database.CanConnect())
                {
                    Debug.WriteLine("Successfully connected to existing database");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Database initialization error: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}