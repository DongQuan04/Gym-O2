using GymOCommunity.Data;
using GymOCommunity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymOCommunity.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DashboardController(ApplicationDbContext context,
                                   UserManager<IdentityUser> userManager,
                                   RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            // Tổng quan
            ViewBag.UserCount = await _userManager.Users.CountAsync();
            ViewBag.PostCount = await _context.Posts.CountAsync();
            ViewBag.CommentCount = await _context.Comments.CountAsync();
            ViewBag.ReportCount = await _context.Reports.CountAsync();

            // Tăng trưởng bài viết theo tháng
            var postGrowthRaw = await _context.Posts
                .GroupBy(p => new { p.CreatedAt.Year, p.CreatedAt.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Count = g.Count()
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            var postGrowth = postGrowthRaw
                .Select(g => new
                {
                    Month = $"{g.Month}/{g.Year}",
                    g.Count
                })
                .ToList();

            ViewBag.PostGrowth = postGrowth;

            // Tăng trưởng bình luận theo tháng
            var commentGrowthRaw = await _context.Comments
                .GroupBy(c => new { c.CreatedAt.Year, c.CreatedAt.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Count = g.Count()
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            var commentGrowth = commentGrowthRaw
                .Select(g => new
                {
                    Month = $"{g.Month}/{g.Year}",
                    g.Count
                })
                .ToList();

            ViewBag.CommentGrowth = commentGrowth;

            // Danh sách năm có bài viết
            ViewBag.Years = await _context.Posts
                .Select(p => p.CreatedAt.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();

            // Người dùng mới (tạm hardcoded ngày vì thiếu CreatedAt)
            var newUsers = await _userManager.Users
                .Where(u => u.LockoutEnd == null && u.EmailConfirmed)
                .OrderByDescending(u => u.Id)
                .Take(5)
                .Select(u => new
                {
                    u.Email,
                    CreatedAt = DateTime.UtcNow.AddDays(-1) // tạm tạo giả CreatedAt
                })
                .ToListAsync();
            ViewBag.NewUsers = newUsers;

            // Hoạt động gần đây
            var activities = new List<string>();

            var latestPosts = await _context.Posts
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .ToListAsync();
            foreach (var post in latestPosts)
                activities.Add($"📝 Bài viết mới: {post.Title}");

            var latestComments = await _context.Comments
                .OrderByDescending(c => c.CreatedAt)
                .Take(5)
                .ToListAsync();
            foreach (var comment in latestComments)
                activities.Add($"💬 Bình luận mới: {comment.Content?.Substring(0, Math.Min(50, comment.Content.Length))}...");

            ViewBag.RecentActivities = activities;

            // Báo cáo gần đây
            var recentReports = await _context.Reports
                .OrderByDescending(r => r.ReportedAt)
                .Take(5)
                .Select(r => $"🚨 Báo cáo bài viết ID: {r.PostId} - Lý do: {r.Description}")
                .ToListAsync();
            ViewBag.Reports = recentReports;

            // Báo động bài viết bị báo cáo nhiều lần
            var flaggedPosts = await _context.Reports
                .GroupBy(r => r.PostId)
                .Where(g => g.Count() >= 3)
                .Select(g => new
                {
                    PostId = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();
            ViewBag.FlaggedPosts = flaggedPosts;
            return View();
        }

    }
}
