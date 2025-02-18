using Microsoft.Extensions.Logging;
using Market.Services;
using Market.ViewModels;
using Market.Converters;
using Market.Views.AddItem;
using Market.Views;
using Market.ViewModels.AddItem;
using Market.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using CommunityToolkit.Maui;
using Microsoft.Extensions.DependencyInjection; 
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

                bool needsRecreation = false;

                if (!File.Exists(dbPath))
                {
                    Debug.WriteLine("Database file not found, creating new database...");
                    needsRecreation = true;
                }
                else
                {
                    try
                    {
                        // Try to execute a query that uses the new columns
                        await context.Items.FirstOrDefaultAsync(i => i.State != null);
                        Debug.WriteLine("Database schema is up to date");
                    }
                    catch (Exception)
                    {
                        Debug.WriteLine("Database schema is outdated, recreating...");
                        needsRecreation = true;
                    }
                }

                if (needsRecreation)
                {
                    await context.RecreateDatabase();
                    Debug.WriteLine("Database created/recreated successfully");
                }

                // Verify final connection
                var canConnect = await context.Database.CanConnectAsync();
                Debug.WriteLine($"Database exists and can connect: {canConnect}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Database initialization error: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
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
                .UseMauiCommunityToolkit()  // Add this line
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
#else
        // Release mode settings
        options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
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

            // add GeoLocationService registration
            builder.Services.AddTransient<IGeolocationService, GeolocationService>();


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
            // In ConfigureServices or similar method
#if WINDOWS
    builder.Services.AddTransient<RentalItemViewModel>();
#endif
            builder.Services.AddTransient<AddItemViewModel>();
            builder.Services.AddTransient<ForSaleItemViewModel>();
            builder.Services.AddTransient<JobItemViewModel>();
            builder.Services.AddTransient<ServiceItemViewModel>();
            builder.Services.AddTransient<SignInViewModel>();
            builder.Services.AddTransient<RegistrationViewModel>();
            builder.Services.AddTransient<MainViewModel>();
            builder.Services.AddTransient<SearchViewModel>();
            builder.Services.AddTransient<InboxViewModel>();
            builder.Services.AddTransient<MyListingsViewModel>();
            builder.Services.AddTransient<PasswordResetViewModel>();
            builder.Services.AddTransient<ItemDetailViewModel>();
        }

        // Registers pages with dependency injection
        // Registers pages with dependency injection
        private static void RegisterPages(MauiAppBuilder builder)
        {
            Debug.WriteLine("Registering Pages...");
            // In ConfigureServices or similar method
            builder.Services.AddTransient<SignInPage>();
            builder.Services.AddTransient<AddItemPage>();
            builder.Services.AddTransient<ForSaleItemPage>();
            builder.Services.AddTransient<RentalItemPage>();
            builder.Services.AddTransient<JobItemPage>();
            builder.Services.AddTransient<ServiceItemPage>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<RegistrationPage>();
            builder.Services.AddTransient<SearchPage>();
            builder.Services.AddTransient<PostItemPage>();
            builder.Services.AddTransient<InboxPage>();
            builder.Services.AddTransient<MyListingsPage>();
            builder.Services.AddTransient<ItemDetailPage>();
        }

        // Configures debug settings for development
        private static void ConfigureDebugSettings(MauiAppBuilder builder)
        {
#if DEBUG
            builder.Logging.AddDebug().SetMinimumLevel(LogLevel.Debug);
#endif
        }

        

    }
}