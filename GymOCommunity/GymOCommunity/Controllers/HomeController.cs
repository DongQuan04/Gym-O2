using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GymOCommunity.Models;
using Microsoft.AspNetCore.Authorization;
using GymOCommunity.Data;

namespace GymOCommunity.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [Authorize]
        public IActionResult Dashboard()
        {
            // Đếm tổng số
            ViewBag.UserCount = _context.Users.Count();
            ViewBag.PostCount = _context.Posts.Count();
            ViewBag.CommentCount = _context.Comments.Count();
            ViewBag.ReportCount = _context.Reports.Count();

            // Lấy danh sách hoạt động gần đây (ví dụ: 5 bài post mới nhất)
            var recentActivities = _context.Posts
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .Select(p => $"New post: {p.Title}")
                .ToList();

            ViewBag.RecentActivities = recentActivities;

            // Dữ liệu tăng trưởng bài viết theo tháng (ĐÃ SỬA)
            var postGrowth = _context.Posts
                .GroupBy(p => new {
                    Year = p.CreatedAt.Year,
                    Month = p.CreatedAt.Month
                })
                .Select(g => new {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count()
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .AsEnumerable() // Chuyển sang client-side để xử lý định dạng
                .Select(x => new {
                    Month = $"{x.Month:00}/{x.Year}", // Format ở phía client
                    x.Count
                })
                .ToList();

            ViewBag.PostGrowth = postGrowth;

            return View();
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}