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

        // Cấu hình các quan hệ giữa các bảng trong phương thức OnModelCreating
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Gọi phương thức base để giữ nguyên các cấu hình Identity

            // Cấu hình quan hệ giữa Post và PostImage
            modelBuilder.Entity<Post>()
                .HasMany(p => p.PostImages)    // Một Post có thể có nhiều PostImage
                .WithOne(pi => pi.Post)       // Mỗi PostImage thuộc về một Post
                .HasForeignKey(pi => pi.PostId);  // Chỉ định khóa ngoại cho PostId
        }
    }
}
