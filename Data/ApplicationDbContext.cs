using Microsoft.EntityFrameworkCore;
using IskoWalkAPI.Models;

namespace IskoWalkAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<WalkRequest> WalkRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            });

            // Configure WalkRequest entity
            modelBuilder.Entity<WalkRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // Relationship: WalkRequest -> User (Requester)
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relationship: WalkRequest -> User (Companion)
                entity.HasOne(e => e.Companion)
                    .WithMany()
                    .HasForeignKey(e => e.CompanionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.Status).HasDefaultValue("Active");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                // Ignore computed properties
                entity.Ignore(e => e.RequesterId);
                entity.Ignore(e => e.RequesterName);
                entity.Ignore(e => e.ToLocation);
                entity.Ignore(e => e.WalkDate);
                entity.Ignore(e => e.WalkTime);
                entity.Ignore(e => e.CompanionName);
            });
        }
    }
}
