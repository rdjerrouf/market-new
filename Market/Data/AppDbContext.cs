using Microsoft.EntityFrameworkCore;
using Market.Models;
using System.Reflection.Emit;



namespace Market.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Message> Messages { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Email).IsRequired();
                entity.Property(u => u.PhoneNumber).IsRequired(false);
                entity.Property(u => u.City).IsRequired(false);
                entity.Property(u => u.Province).IsRequired(false);
            });

            // Item configuration
            modelBuilder.Entity<Item>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.Title).IsRequired();
                entity.Property(i => i.Description).IsRequired();
                entity.Property(i => i.PhotoUrl).IsRequired(false);
                entity.HasOne<User>()
                      .WithMany()
                      .HasForeignKey(i => i.UserId);
            });

            // Message configuration
            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.Property(m => m.Content).IsRequired();
                entity.Property(m => m.Timestamp).IsRequired();
                entity.HasOne<User>()
                      .WithMany()
                      .HasForeignKey(m => m.SenderId);
                entity.HasOne<User>()
                      .WithMany()
                      .HasForeignKey(m => m.ReceiverId);
            });
        }

        public async Task MigrateAsync()
        {
            await Database.MigrateAsync();
        }
    }
}