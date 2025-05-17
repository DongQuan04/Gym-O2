using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GymOCommunity.Data;

namespace GymOCommunity.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PostsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var posts = _context.Posts
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            return View(posts);
        }

        public IActionResult Delete(int id)
        {
            var post = _context.Posts.Find(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
