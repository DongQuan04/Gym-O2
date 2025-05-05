using GymOCommunity.Data;
using GymOCommunity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GymOCommunity.Controllers
{
    [Authorize]
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public PostsController(
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            var posts = _context.Posts.ToList();
            return View(posts);
        }

        [AllowAnonymous]
        public IActionResult Details(int id)
        {
            var post = _context.Posts
                .Include(p => p.Comments)
                .Include(p => p.PostImages)
                .Include(p => p.PostVideos)
                .FirstOrDefault(p => p.Id == id);

            if (post == null)
                return NotFound();

            return View(post);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [RequestSizeLimit(104857600)] // 100MB
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Post post)
        {
            var currentUserId = _userManager.GetUserId(User);
            post.UserId = currentUserId;
            post.CreatedAt = DateTime.Now;

            if (!ModelState.IsValid)
                return View(post);

            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            // Ảnh đại diện
            if (post.ImageFile != null && post.ImageFile.Length > 0)
            {
                string uniqueFileName = Guid.NewGuid() + "_" + Path.GetFileName(post.ImageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await post.ImageFile.CopyToAsync(stream);
                }

                post.ImageUrl = "/uploads/" + uniqueFileName;
            }

            _context.Posts.Add(post);
            await _context.SaveChangesAsync(); // Cần lưu trước để có post.Id

            // Thêm video
            if (post.VideoFiles != null && post.VideoFiles.Any())
            {
                foreach (var video in post.VideoFiles)
                {
                    if (video.Length > 0)
                    {
                        string uniqueVideoName = Guid.NewGuid() + "_" + Path.GetFileName(video.FileName);
                        string videoPath = Path.Combine(uploadsFolder, uniqueVideoName);

                        using (var stream = new FileStream(videoPath, FileMode.Create))
                        {
                            await video.CopyToAsync(stream);
                        }

                        var postVideo = new PostVideo
                        {
                            PostId = post.Id,
                            VideoUrl = "/uploads/" + uniqueVideoName
                        };

                        _context.PostVideos.Add(postVideo);
                    }
                }
            }

            // Thêm ảnh bổ sung
            if (post.AdditionalImages != null && post.AdditionalImages.Any())
            {
                foreach (var image in post.AdditionalImages)
                {
                    if (image.Length > 0)
                    {
                        string uniqueFileName = Guid.NewGuid() + "_" + Path.GetFileName(image.FileName);
                        string imagePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var stream = new FileStream(imagePath, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }

                        var postImage = new PostImage
                        {
                            PostId = post.Id,
                            ImageUrl = "/uploads/" + uniqueFileName
                        };

                        _context.PostImages.Add(postImage);
                    }
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var post = _context.Posts.Find(id);
            if (post == null)
                return NotFound();

            if (!IsAuthorized(post.UserId))
                return Forbid();

            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Post post)
        {
            var existingPost = await _context.Posts
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == post.Id);

            if (existingPost == null)
                return NotFound();

            if (!IsAuthorized(existingPost.UserId))
                return Forbid();

            if (!ModelState.IsValid)
                return View(post);

            string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");

            // Ảnh đại diện
            if (post.ImageFile != null && post.ImageFile.Length > 0)
            {
                string uniqueFileName = $"{Guid.NewGuid()}_{post.ImageFile.FileName}";
                string filePath = Path.Combine(uploadsDir, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await post.ImageFile.CopyToAsync(stream);
                }

                post.ImageUrl = $"/uploads/{uniqueFileName}";

                if (!string.IsNullOrEmpty(existingPost.ImageUrl))
                {
                    string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, existingPost.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                        System.IO.File.Delete(oldFilePath);
                }
            }
            else
            {
                post.ImageUrl = existingPost.ImageUrl;
            }

            post.UserId = existingPost.UserId;
            post.CreatedAt = existingPost.CreatedAt;

            try
            {
                _context.Update(post);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi cập nhật bài viết: {ex.Message}");
                ModelState.AddModelError("", "Đã xảy ra lỗi khi cập nhật bài viết.");
                return View(post);
            }
        }

        public IActionResult Delete(int id)
        {
            var post = _context.Posts.Find(id);
            if (post == null)
                return NotFound();

            if (!IsAuthorized(post.UserId))
                return Forbid();

            return View(post);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
                return NotFound();

            if (!IsAuthorized(post.UserId))
                return Forbid();

            try
            {
                // Xóa ảnh đại diện
                if (!string.IsNullOrEmpty(post.ImageUrl))
                {
                    string filePath = Path.Combine(_webHostEnvironment.WebRootPath, post.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                }

                // Xóa video
                if (!string.IsNullOrEmpty(post.VideoUrl))
                {
                    string videoPath = Path.Combine(_webHostEnvironment.WebRootPath, post.VideoUrl.TrimStart('/'));
                    if (System.IO.File.Exists(videoPath))
                        System.IO.File.Delete(videoPath);
                }

                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi xóa bài viết: {ex.Message}");
                ModelState.AddModelError("", "Đã xảy ra lỗi khi xóa bài viết.");
                return View("Delete", post);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int postId, string content)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
                return NotFound();

            var comment = new Comment
            {
                PostId = postId,
                Content = content,
                CreatedAt = DateTime.Now,
                UserName = User.Identity?.Name ?? "Anonymous",
                UserId = _userManager.GetUserId(User) ?? "Anonymous"
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = postId });
        }

        private bool IsAuthorized(string postOwnerId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");
            return isAdmin || postOwnerId == currentUserId;
        }
    }
}
