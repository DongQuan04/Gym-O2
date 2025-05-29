using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GymOCommunity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using GymOCommunity.Models;
using System.Threading.Tasks;

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
                .Include(p => p.User) // Load thông tin người dùng
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
                .Include(p => p.PostImages) // Load ảnh đính kèm
                .Include(p => p.PostVideos) // Load video đính kèm
                .FirstOrDefaultAsync(m => m.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            // Tăng lượt xem nếu cần
            // post.ViewCount = (post.ViewCount ?? 0) + 1;
            // await _context.SaveChangesAsync();

            return View(post);
        }

        // POST: Admin/Posts/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            try
            {
                // Xóa các media liên quan trước
                var images = await _context.PostImages.Where(pi => pi.PostId == id).ToListAsync();
                var videos = await _context.PostVideos.Where(pv => pv.PostId == id).ToListAsync();

                _context.PostImages.RemoveRange(images);
                _context.PostVideos.RemoveRange(videos);
                _context.Posts.Remove(post);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Bài viết đã được xóa thành công!";
            }
            catch (DbUpdateException ex)
            {
                TempData["ErrorMessage"] = "Không thể xóa bài viết. Có thể có dữ liệu liên quan.";
                // Log the error (uncomment ex variable name and write a log.)
            }

            return RedirectToAction(nameof(Index));
        }

        // Thêm các action khác nếu cần
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