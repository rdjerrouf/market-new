using Market.DataAccess.Models;
using System.Diagnostics;
using Market.Market.DataAccess.Models;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore; // Add this using directive
using Microsoft.EntityFrameworkCore.Storage.ValueConversion; // For enum conversion

namespace Market.DataAccess.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Message> Messages { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            Database.EnsureCreated();
            // Enable detailed logging in debug mode
            Debug.WriteLine("AppDbContext constructor called");
        }

        // method in the AppDbContext to recreate the database
        public async Task RecreateDatabase()
        {
            await Database.EnsureDeletedAsync();
            await Database.EnsureCreatedAsync();
            Debug.WriteLine("Database recreated successfully");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            Debug.WriteLine("Configuring database models...");

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users"); // Explicitly specify table name
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();

                // Required fields
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired();

                // Optional fields
                entity.Property(e => e.PhoneNumber).HasMaxLength(20).IsRequired(false);
                entity.Property(e => e.City).HasMaxLength(50).IsRequired(false);
                entity.Property(e => e.Province).HasMaxLength(50).IsRequired(false);

                // Set default value for CreatedAt
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<Item>(entity =>
            {
                entity.ToTable("Items");
                entity.HasKey(e => e.Id);

                // Required fields
                entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Category).IsRequired();

                // Optional fields
                entity.Property(e => e.PhotoUrl).IsRequired(false);
                entity.Property(e => e.JobType).IsRequired(false);
                entity.Property(e => e.ServiceType).IsRequired(false);
                entity.Property(e => e.RentalPeriod).IsRequired(false);
                entity.Property(e => e.AvailableFrom).IsRequired(false);
                entity.Property(e => e.AvailableTo).IsRequired(false);

                // for location-based and state -based search

                entity.Property(e => e.State)
       .HasConversion(
           v => v.HasValue ? v.Value.ToString() : null,
           v => !string.IsNullOrEmpty(v) ?
               (AlState)Enum.Parse(typeof(AlState), v) :
               (AlState?)null
       )
       .IsRequired(false);

                entity.Property(e => e.Latitude)
                    .HasColumnType("decimal(10,6)")
                    .IsRequired(false);

                entity.Property(e => e.Longitude)
                    .HasColumnType("decimal(10,6)")
                    .IsRequired(false);

                // Create an index for location-based queries
                entity.HasIndex(e => new { e.Latitude, e.Longitude });
                // New Job-specific fields
                entity.Property(e => e.JobCategory)
                    .HasConversion(
                        v => v.HasValue ? v.Value.ToString() : null,
                        v => !string.IsNullOrEmpty(v) ?
                            (JobCategory)Enum.Parse(typeof(JobCategory), v) :
                            (JobCategory?)null
                    )
                    .IsRequired(false);

                entity.Property(e => e.CompanyName)
                    .HasMaxLength(100)
                    .IsRequired(false);

                entity.Property(e => e.JobLocation)
                    .HasMaxLength(200)
                    .IsRequired(false);

                entity.Property(e => e.ApplyMethod)
                    .HasConversion(
                        v => v.HasValue ? v.Value.ToString() : null,
                        v => !string.IsNullOrEmpty(v) ?
                            (ApplyMethod)Enum.Parse(typeof(ApplyMethod), v) :
                            (ApplyMethod?)null
                    )
                    .IsRequired(false);

                entity.Property(e => e.ApplyContact)
                    .HasMaxLength(200)
                    .IsRequired(false);

                // New Service-specific fields
                entity.Property(e => e.ServiceCategory)
                    .HasConversion(
                        v => v.HasValue ? v.Value.ToString() : null,
                        v => !string.IsNullOrEmpty(v) ?
                            (ServiceCategory)Enum.Parse(typeof(ServiceCategory), v) :
                            (ServiceCategory?)null
                    )
                    .IsRequired(false);

                entity.Property(e => e.ServiceAvailability)
                    .HasConversion(
                        v => v.HasValue ? v.Value.ToString() : null,
                        v => !string.IsNullOrEmpty(v) ?
                            (ServiceAvailability)Enum.Parse(typeof(ServiceAvailability), v) :
                            (ServiceAvailability?)null
                    )
                    .IsRequired(false);

                entity.Property(e => e.YearsOfExperience).IsRequired(false);

                entity.Property(e => e.NumberOfEmployees).IsRequired(false);

                entity.Property(e => e.ServiceLocation)
                    .HasMaxLength(200)
                    .IsRequired(false);

                entity.Property(e => e.AverageRating)
                    .HasColumnType("decimal(3,2)")
                    .IsRequired(false);

                // Generated fields
                entity.Property(e => e.ListedDate)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAdd();

                // Add new fields for sale and rent categories
                entity.Property(e => e.ForSaleCategory)
        .HasConversion(
            v => v.HasValue ? v.Value.ToString() : null,
            v => !string.IsNullOrEmpty(v) ?
                (ForSaleCategory)Enum.Parse(typeof(ForSaleCategory), v) :
                (ForSaleCategory?)null
        )
        .IsRequired(false);

                entity.Property(e => e.ForRentCategory)
                    .HasConversion(
                        v => v.HasValue ? v.Value.ToString() : null,
                        v => !string.IsNullOrEmpty(v) ?
                            (ForRentCategory)Enum.Parse(typeof(ForRentCategory), v) :
                            (ForRentCategory?)null
                    )
                    .IsRequired(false);
            });

            modelBuilder.Entity<Message>(entity =>
            {
                entity.ToTable("Messages");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.SenderId).IsRequired();
                entity.Property(e => e.ReceiverId).IsRequired();
                entity.Property(e => e.Timestamp)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAdd();
                entity.Property(e => e.IsRead).HasDefaultValue(false);
                entity.Property(e => e.RelatedItemId).IsRequired(false);
            });

            Debug.WriteLine("Database model configuration completed");
        }



    }
}