using Microsoft.Extensions.Logging;
using Market.Services;
using Market.ViewModels;
using Market.Converters;
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

        // Initializes the database with required schema
        private static async Task InitializeDatabase(MauiApp app)
        {
            Debug.WriteLine("Initializing database...");

            using var scope = app.Services.CreateScope();
            try
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var dbPath = Path.Combine(FileSystem.AppDataDirectory, "market.db");

                // Delete all SQLite files if they exist
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
                if (File.Exists(dbPath + "-shm"))
                    File.Delete(dbPath + "-shm");
                if (File.Exists(dbPath + "-wal"))
                    File.Delete(dbPath + "-wal");

                Debug.WriteLine("Existing database files deleted");

                // Create new database with updated schema
                await context.Database.EnsureCreatedAsync();
                Debug.WriteLine("New database created successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Database initialization error: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        // Creates and configures the MAUI app to handle the async operation
        public static MauiApp CreateMauiApp()
        {
            Debug.WriteLine("Starting application initialization...");

            var builder = MauiApp.CreateBuilder();

            ConfigureBasicSettings(builder);
            ConfigureDatabase(builder);
            RegisterServices(builder);
            RegisterViewModels(builder);
            RegisterPages(builder);
            ConfigureDebugSettings(builder);

            var app = builder.Build();

            // Initialize database asynchronously but wait for it to complete
            Task.Run(async () => await InitializeDatabase(app)).GetAwaiter().GetResult();

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
            // Converter Registrations (Add these lines HERE)
            builder.Services.AddTransient<StringToBoolConverter>();
            builder.Services.AddTransient<StringEqualityConverter>();
            builder.Services.AddTransient<InverseBoolConverter>();
            builder.Services.AddTransient<StringNotNullOrEmptyBoolConverter>();
            builder.Services.AddTransient<BoolToColorConverter>();
            builder.Services.AddTransient<BoolToFontAttributesConverter>();
            Debug.WriteLine("Converters registered."); // Add a debug log for verification.
        }   // Registers view models with dependency injection
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
        
    }
}