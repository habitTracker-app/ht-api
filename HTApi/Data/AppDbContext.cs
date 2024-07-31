using HTAPI.Models;
using HTAPI.Models.ChallengeGoals;
using HTAPI.Models.Challenges;
using HTAPI.Models.DemographicData;
using HTAPI.Models.Friendships;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HTAPI.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }


        // Goals
        public DbSet<Frequency> Frequency { get; set; }
        public DbSet<Goal> Goal { get; set; }
        public DbSet<Progress> Progress { get; set; }

        // Challenges
        public DbSet<Challenge> Challenge { get; set; }
        public DbSet<ChallengeCategory> ChallengeCategory { get; set; }
        
        // Demographic Data 
        public DbSet<Country> Country { get; set; }
        public DbSet<Gender> Gender { get; set; }
        
        // Friendships 
        public DbSet<Friendship> Friendship { get; set; }
        public DbSet<FriendshipStatus> FriendshipStatus { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            
            optionsBuilder.UseNpgsql("Host=localhost; Port=5432; Database=habitTracker-api-db; Username=; Password=; Client Encoding=UTF8");
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            // Country config
            builder.Entity<Country>()
                .HasIndex(c => c.Name)
                .IsUnique();
            builder.Entity<Country>()
                .HasIndex(c => c.NativeName)
                .IsUnique();
            builder.Entity<Country>()
                .HasIndex(c => c.Abbreviation)
                .IsUnique();
            builder.Entity<Country>()
                .Property(c => c.FlagEmoji)
                .IsUnicode(true);


            // Gender config
            builder.Entity<Gender>()
                .HasIndex(g => g.Name)
                .IsUnique();
            builder.Entity<Gender>()
                .HasIndex(g => g.FlagEmoji)
                .IsUnique();
            builder.Entity<Gender>()
                .Property(c => c.FlagEmoji)
                .IsUnicode(true);

            // Challenge Category config
            builder.Entity<ChallengeCategory>()
                .HasIndex(c => c.Name)
                .IsUnique();
            builder.Entity<ChallengeCategory>()
                .HasIndex(c => c.Description)
                .IsUnique();

            // Challenge config
            builder.Entity<Challenge>()
                .HasOne(c => c.Owner)
                .WithMany()
                .HasForeignKey(c => c.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder.Entity<Challenge>()
               .HasOne(c => c.Category)
               .WithMany()
               .HasForeignKey(c => c.CategoryId)
               .OnDelete(DeleteBehavior.SetNull);

            // Friendship config
            builder.Entity<Friendship>()
                .HasOne(f => f.Requester)
                .WithMany()
                .HasForeignKey(f => f.RequesterId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<Friendship>()
                .HasOne(f => f.Target)
                .WithMany()
                .HasForeignKey(f => f.TargetId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<Friendship>()
                .HasOne(f => f.Status)
                .WithMany()
                .HasForeignKey(f => f.StatusId)
                .OnDelete(DeleteBehavior.SetNull);


            // Friendship Status config
            builder.Entity<FriendshipStatus>()
                .HasIndex(f => f.Status)
                .IsUnique();

            // User config
            builder.Entity<User>()
                .HasIndex(u => u.UUID)
                .IsUnique();
            builder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasColumnType("timestamp(6)");
            builder.Entity<User>()
               .Property(u => u.UpdatedAt)
               .HasColumnType("timestamp(6)");

            // Frequency config
            builder.Entity<Frequency>()
                .HasIndex(f => f.Type)
                .IsUnique();
        }


    }
}
