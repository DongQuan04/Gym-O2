using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GymOCommunity.Models;

namespace GymOCommunity.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Các DbSet đại diện cho các bảng trong cơ sở dữ liệu
        public DbSet<Post> Posts { get; set; } // Bảng Post trong database
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<PostImage> PostImages { get; set; }
        public DbSet<PostVideo> PostVideos { get; set; }


        // Cấu hình các quan hệ giữa các bảng trong phương thức OnModelCreating
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Gọi phương thức base để giữ nguyên các cấu hình Identity

            // Cấu hình quan hệ giữa Post và PostImage
            modelBuilder.Entity<Post>()
                .HasMany(p => p.PostImages)    // Một Post có thể có nhiều PostImage
                .WithOne(pi => pi.Post)       // Mỗi PostImage thuộc về một Post
                .HasForeignKey(pi => pi.PostId);  // Chỉ định khóa ngoại cho PostId


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
        }
    }
}
