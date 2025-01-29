// Data/AppDbContextFactory.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.IO;

namespace Market.DataAccess.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            // Get the app's local data directory
            string folderPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Market");

            // Ensure the path exists and is not null
            if (!string.IsNullOrEmpty(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                string dbPath = Path.Combine(folderPath, "market.db");
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }
            else
            {
                // Fallback to a default path if needed
                optionsBuilder.UseSqlite("Data Source=market.db");
            }

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}