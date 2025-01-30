using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Diagnostics;

namespace Market.DataAccess.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            try
            {
                // Get the app's local data directory
                string folderPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Market");

                Debug.WriteLine($"Database folder path: {folderPath}");

                // Ensure the directory exists
                if (!Directory.Exists(folderPath))
                {
                    Debug.WriteLine("Creating database directory");
                    Directory.CreateDirectory(folderPath);
                }

                string dbPath = Path.Combine(folderPath, "market.db");
                Debug.WriteLine($"Database file path: {dbPath}");

                // Check if database file exists
                if (File.Exists(dbPath))
                {
                    Debug.WriteLine("Database file already exists");
                }
                else
                {
                    Debug.WriteLine("Database file does not exist yet");
                }

                optionsBuilder.UseSqlite($"Data Source={dbPath}");
                Debug.WriteLine("Database connection string configured");

                return new AppDbContext(optionsBuilder.Options);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in AppDbContextFactory: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }
    }
}