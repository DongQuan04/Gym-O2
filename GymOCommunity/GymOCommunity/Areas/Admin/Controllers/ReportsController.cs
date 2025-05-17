using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GymOCommunity.Data;
using Microsoft.EntityFrameworkCore;

namespace GymOCommunity.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var reports = await _context.Reports
                .Include(r => r.Post) // nếu có navigation property
                .OrderByDescending(r => r.ReportedAt)
                .ToListAsync();

            return View(reports);
        }
    }
}
