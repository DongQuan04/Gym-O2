using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GymOCommunity.Models;
using Microsoft.CodeAnalysis;

namespace GymOCommunity.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Các DbSet đại diện cho các bảng trong cơ sở dữ liệu
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<PostImage> PostImages { get; set; }
        public DbSet<PostVideo> PostVideos { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<WorkoutLog> WorkoutLogs { get; set; }
        public DbSet<SharedPost> SharedPosts { get; set; }

        public DbSet<Notification> Notifications { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Gọi base Identity

            modelBuilder.Entity<Post>()
                .HasMany(p => p.PostImages)
                .WithOne(pi => pi.Post)
                .HasForeignKey(pi => pi.PostId);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserProfile>()
                .HasOne<IdentityUser>()
                .WithOne()
                .HasForeignKey<UserProfile>(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserProfile>()
                .HasIndex(up => up.UserId)
                .IsUnique();

            //  Cấu hình ReportedAt
            modelBuilder.Entity<Report>()
                .Property(r => r.ReportedAt)
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETUTCDATE()");

            // Cấu hình xóa Notification khi Post bị xóa
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Post)
                .WithMany(p => p.Notifications)
                .HasForeignKey(n => n.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}