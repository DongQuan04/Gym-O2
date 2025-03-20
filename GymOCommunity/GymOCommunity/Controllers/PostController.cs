using GymOCommunity.Data;
using GymOCommunity.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace GymOCommunity.Controllers
{
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PostsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Danh sách bài viết
        public IActionResult Index()
        {
            var posts = _context.Posts.ToList();
            return View(posts);
        }

        // Chi tiết bài viết
        public IActionResult Details(int id)
        {
            var post = _context.Posts.Find(id);
            if (post == null) return NotFound();
            return View(post);
        }

        // Hiển thị form tạo bài viết
        public IActionResult Create()
        {
            return View();
        }

        // Xử lý khi submit form tạo bài viết
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Post post)
        {
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState không hợp lệ!");

                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine("Lỗi: " + error.ErrorMessage);
                }

                return View(post);
            }

            _context.Posts.Add(post);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }


        // Hiển thị form chỉnh sửa bài viết
        public IActionResult Edit(int id)
        {
            var post = _context.Posts.Find(id);
            if (post == null) return NotFound();
            return View(post);
        }

        // Xử lý khi submit form chỉnh sửa bài viết
        [HttpPost]
        public IActionResult Edit(Post post)
        {
            if (ModelState.IsValid)
            {
                _context.Update(post);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(post);
        }

        // Hiển thị trang xác nhận xóa
        public IActionResult Delete(int id)
        {
            var post = _context.Posts.Find(id);
            if (post == null) return NotFound();
            return View(post);
        }

        // Xử lý khi xác nhận xóa bài viết
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var post = _context.Posts.Find(id);
            if (post == null) return NotFound();
            _context.Posts.Remove(post);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
