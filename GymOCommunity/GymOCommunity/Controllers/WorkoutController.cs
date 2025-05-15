using GymOCommunity.Models;
using GymOCommunity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

[Authorize]
public class WorkoutController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public WorkoutController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var logs = _context.WorkoutLogs
                           .Where(x => x.UserId == user.Id)
                           .ToList();

        // Tổng số buổi theo nhóm cơ
        ViewBag.MuscleStats = logs
            .GroupBy(l => l.MuscleGroup)
            .Select(g => new { MuscleGroup = g.Key, Count = g.Count() })
            .ToList();

        // Tổng số reps mỗi ngày (dùng để hiển thị biểu đồ tiến bộ)
        ViewBag.ProgressStats = logs
            .GroupBy(l => l.Date.Date)
            .Select(g => new { Date = g.Key, TotalReps = g.Sum(x => x.Sets * x.Reps) })
            .OrderBy(x => x.Date)
            .ToList();

        // Danh sách nhóm cơ duy nhất để filter
        ViewBag.MuscleGroups = logs.Select(x => x.MuscleGroup).Distinct().ToList();

        return View(logs);
    }


    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(WorkoutLog log)
    {
        if (!ModelState.IsValid)
            return View(log);

        var user = await _userManager.GetUserAsync(User);
        log.UserId = user.Id;
        _context.WorkoutLogs.Add(log);
        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }
}
