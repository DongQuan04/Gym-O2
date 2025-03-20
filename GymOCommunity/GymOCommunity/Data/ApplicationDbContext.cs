using GymOCommunity.Models;
using Microsoft.EntityFrameworkCore;

namespace GymOCommunity.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Post> Posts { get; set; } // Bảng Post trong database
    }
}
