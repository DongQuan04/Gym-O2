using GymOCommunity.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymOCommunity.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public NotificationController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> GetRecentNotifications()
        {
            var userId = _userManager.GetUserId(User);
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(10)
                .Include(n => n.TriggerUser)
                .Include(n => n.Post)
                .ToListAsync();

            return PartialView("_NotificationList", notifications);
        }

        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = _userManager.GetUserId(User);
            var count = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .CountAsync();

            return Content(count.ToString());
        }


        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
                return Ok();
            }
            return NotFound();
        }
    }
}
