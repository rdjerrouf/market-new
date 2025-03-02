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
        public DbSet<ItemLocation> ItemLocations { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<ItemStatistics> ItemStatistics { get; set; }
        public DbSet<ItemPhoto> ItemPhotos { get; set; }
        public DbSet<VerificationToken> VerificationTokens { get; set; }
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
                // User configuration

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

                // Configure the User-FavoriteItems relationship
                entity.HasMany(u => u.FavoriteItems)
                    .WithMany(i => i.FavoritedByUsers)
                    .UsingEntity(j => j.ToTable("UserFavorites"));
            });
            // Item configuration
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

                entity.HasOne(i => i.PostedByUser)
                    .WithMany(u => u.PostedItems)
                    .HasForeignKey(i => i.PostedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            // Message configuration
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
            // Rating configuration
            modelBuilder.Entity<Rating>(entity =>
            {
                entity.ToTable("Ratings");
                entity.HasKey(e => e.Id);

                // Configure relationships
                entity.HasOne(r => r.User)
                    .WithMany()
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Item)
                    .WithMany()
                    .HasForeignKey(r => r.ItemId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Constraints
                entity.Property(e => e.Score)
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAdd();

                // Optional fields
                entity.Property(e => e.Review)
                    .HasMaxLength(1000)
                    .IsRequired(false);

                entity.Property(e => e.IsVerifiedPurchase)
                    .HasDefaultValue(false);

                entity.Property(e => e.HelpfulVotes)
                    .HasDefaultValue(0);
            });

            // Item photo model
            modelBuilder.Entity<ItemPhoto>(entity =>
            {
                entity.ToTable("ItemPhotos");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.PhotoUrl)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.DisplayOrder)
                    .HasDefaultValue(0);

                entity.Property(e => e.IsPrimaryPhoto)
                    .HasDefaultValue(false);

                entity.Property(e => e.UploadedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAdd();

                // Configure relationship with Item
                entity.HasOne(p => p.Item)
                    .WithMany(i => i.Photos)
                    .HasForeignKey(p => p.ItemId)
                    .OnDelete(DeleteBehavior.Cascade);
            });


            // Item statistics model

            modelBuilder.Entity<ItemStatistics>(entity =>
            {
                entity.ToTable("ItemStatistics");
                entity.HasKey(e => e.Id);

                entity.HasOne(s => s.Item)
                    .WithOne()
                    .HasForeignKey<ItemStatistics>(s => s.ItemId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(e => e.ViewCount).HasDefaultValue(0);
                entity.Property(e => e.InquiryCount).HasDefaultValue(0);
                entity.Property(e => e.FavoriteCount).HasDefaultValue(0);
                entity.Property(e => e.FirstViewedAt).IsRequired(false);
                entity.Property(e => e.LastViewedAt).IsRequired(false);
            });

            // Verification token model
            modelBuilder.Entity<VerificationToken>(entity =>
            {
                entity.ToTable("VerificationTokens");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Token).IsRequired();
                entity.Property(e => e.Type).IsRequired();
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAdd();
                entity.Property(e => e.ExpiresAt).IsRequired();
                entity.Property(e => e.IsUsed).HasDefaultValue(false);

                entity.HasOne(vt => vt.User)
                    .WithMany()
                    .HasForeignKey(vt => vt.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Item geo-location model
            modelBuilder.Entity<ItemLocation>()
                .HasOne(il => il.Item)
                .WithOne()
                .HasForeignKey<ItemLocation>(il => il.ItemId);

        
            Debug.WriteLine("Database model configuration completed");
        }



    }
}