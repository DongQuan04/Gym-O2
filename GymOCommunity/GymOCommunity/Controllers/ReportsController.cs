using GymOCommunity.Data;
using GymOCommunity.Models;
using Microsoft.AspNetCore.Mvc;

namespace GymOCommunity.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Create(int postId)
        {
            ViewBag.PostId = postId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Report report)
        {
            if (ModelState.IsValid)
            {
                report.ReportedAt = DateTime.UtcNow;
                _context.Reports.Add(report);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Posts", new { id = report.PostId });
            }
            return View(report);
        }
    }

}
