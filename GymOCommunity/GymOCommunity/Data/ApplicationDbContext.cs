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





        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Gọi phương thức base để giữ nguyên các cấu hình Identity

            // Cấu hình quan hệ giữa Post và PostImage
            modelBuilder.Entity<Post>()
                .HasMany(p => p.PostImages)
                .WithOne(pi => pi.Post)
                .HasForeignKey(pi => pi.PostId);

            // Cấu hình quan hệ Post - Comment
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình quan hệ Comment - User (IdentityUser)
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình quan hệ 1-1 giữa IdentityUser và UserProfile
            modelBuilder.Entity<UserProfile>()
                .HasOne<IdentityUser>()
                .WithOne()
                .HasForeignKey<UserProfile>(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình index cho UserId trong UserProfile để đảm bảo mỗi user chỉ có 1 profile
            modelBuilder.Entity<UserProfile>()
                .HasIndex(up => up.UserId)
                .IsUnique();
        }
    }
}