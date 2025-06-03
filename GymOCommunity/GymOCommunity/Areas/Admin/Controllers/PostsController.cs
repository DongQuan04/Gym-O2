using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GymOCommunity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using GymOCommunity.Models;
using System.Threading.Tasks;
using System.Linq;

namespace GymOCommunity.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PostsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Admin/Posts
        public async Task<IActionResult> Index()
        {
            var posts = await _context.Posts
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(posts);
        }

        // GET: Admin/Posts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.PostImages)
                .Include(p => p.PostVideos)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // GET: Admin/Posts/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return BadRequest();

            var post = await _context.Posts
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
                return NotFound();

            return View(post); // Truyền model sang view xác nhận xoá
        }

        // POST: Admin/Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
                return NotFound();

            try
            {
                var images = await _context.PostImages.Where(pi => pi.PostId == id).ToListAsync();
                var videos = await _context.PostVideos.Where(pv => pv.PostId == id).ToListAsync();

                _context.PostImages.RemoveRange(images);
                _context.PostVideos.RemoveRange(videos);
                _context.Posts.Remove(post);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Bài viết đã được xóa thành công!";
            }
            catch
            {
                TempData["ErrorMessage"] = "Lỗi khi xóa bài viết.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleFeatured(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            post.IsFeatured = !post.IsFeatured;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
