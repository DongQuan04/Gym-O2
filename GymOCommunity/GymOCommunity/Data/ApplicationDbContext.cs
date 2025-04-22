using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GymOCommunity.Models;

namespace GymOCommunity.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Post> Posts { get; set; } // Bảng Post trong database
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Report> Reports { get; set; }


    }
}

