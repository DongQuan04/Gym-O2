using GymOCommunity.Data;
using GymOCommunity.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace GymOCommunity.Controllers
{
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PostsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            var posts = _context.Posts.ToList();
            return View(posts);
        }

        public IActionResult Details(int id)
        {
            var post = _context.Posts
                .Include(p => p.Comments) // Bình luận được lấy cùng với bài post
                .FirstOrDefault(p => p.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }



        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Post post, IFormFile ImageFile)
        {
            if (!ModelState.IsValid)
            {
                return View(post);
            }

            if (ImageFile != null && ImageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + ImageFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    ImageFile.CopyTo(fileStream);
                }

                post.ImageUrl = "/uploads/" + uniqueFileName;
            }

            _context.Posts.Add(post);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var post = _context.Posts.Find(id);
            if (post == null) return NotFound();
            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Post post, IFormFile ImageFile)
        {
            if (!ModelState.IsValid)
            {
                return View(post);
            }

            if (ImageFile != null && ImageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + ImageFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    ImageFile.CopyTo(fileStream);
                }

                post.ImageUrl = "/uploads/" + uniqueFileName;
            }

            _context.Update(post);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var post = _context.Posts.Find(id);
            if (post == null) return NotFound();
            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddComment(int postId, string content)
        {
            Console.WriteLine($"📌 DEBUG: Nhận comment - PostId: {postId}, Content: {content}");

            var post = _context.Posts.Find(postId);
            if (post == null)
            {
                Console.WriteLine("⚠️ DEBUG: Không tìm thấy bài viết!");
                return NotFound();
            }

            string userName = User.Identity.IsAuthenticated ? User.Identity.Name : "Anonymous";
            string userId = User.Identity.IsAuthenticated ? GetUserId(User.Identity.Name) : "Anonymous";

            Console.WriteLine($"👤 DEBUG: Người dùng - UserName: {userName}, UserId: {userId}");

            var comment = new Comment
            {
                PostId = postId,
                Content = content,
                CreatedAt = DateTime.Now,
                UserName = userName,
                UserId = userId
            };

            _context.Comments.Add(comment);
            _context.SaveChanges();

            Console.WriteLine("✅ DEBUG: Bình luận đã được lưu!");

            return RedirectToAction("Details", new { id = postId });
        }

        private string GetUserId(string userName)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserName == userName);
            return user != null ? user.Id : "Anonymous";
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var post = _context.Posts.Find(id);
            if (post == null) return NotFound();

            if (!string.IsNullOrEmpty(post.ImageUrl))
            {
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, post.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.Posts.Remove(post);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
