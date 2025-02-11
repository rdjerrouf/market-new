using Market.DataAccess.Models;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore; // Add this using directive

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

                // Generated fields
                entity.Property(e => e.ListedDate)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAdd();
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