using GymOCommunity.Data;
using GymOCommunity.Models;
using GymOCommunity.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GymOCommunity.Controllers
{
    [Authorize]
    public class PostsController : Controller
    {
        private readonly ILogger<PostsController> _logger;
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
            var viewModel = new PostListViewModel
            {
                Posts = posts
            };
            return View(viewModel);
        }

        [AllowAnonymous]
        public IActionResult Details(int id)
        {
            var post = _context.Posts
                .Include(p => p.Comments)
                        .ThenInclude(c => c.User)
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LikeComment(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            comment.Likes++;
            await _context.SaveChangesAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                // Trả về JSON nếu là AJAX request
                return Json(new { newLikeCount = comment.Likes });
            }

            // Fallback cho trình duyệt không hỗ trợ JavaScript
            return RedirectToAction("Details", "Posts", new { id = comment.PostId });
        }

        [HttpPost]
        [RequestSizeLimit(1073741824)] // 1GB
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

            return RedirectToAction("Index", "Profile", new { userId = post.UserId });


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

        // Phương thức sửa đổi (kết hợp cả 2 phương thức AddComment)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(100 * 1024 * 1024)] // Giới hạn 100MB cho request
        public async Task<IActionResult> AddComment(int postId, string content, int? parentCommentId = null, IFormFile videoFile = null)
        {
            try
            {
                var post = await _context.Posts.FindAsync(postId);
                if (post == null) return NotFound();

                // Validate video size
                if (videoFile != null && videoFile.Length > 50 * 1024 * 1024) // 50MB
                {
                    TempData["ErrorMessage"] = "Video không được vượt quá 50MB";
                    return RedirectToAction("Details", new { id = postId });
                }

                var comment = new Comment
                {
                    Content = content,
                    PostId = postId,
                    ParentCommentId = parentCommentId,
                    UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    UserName = User.Identity?.Name,
                    CreatedAt = DateTime.Now
                };

                // Xử lý upload video
                if (videoFile != null && videoFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "comment_videos");

                    // Tạo thư mục nếu chưa tồn tại
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Tạo tên file duy nhất
                    var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(videoFile.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Lưu file
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await videoFile.CopyToAsync(fileStream);
                    }

                    comment.VideoUrl = $"/uploads/comment_videos/{uniqueFileName}";
                }

                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();

                return RedirectToAction("Details", new { id = postId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm bình luận có video");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi gửi bình luận";
                return RedirectToAction("Details", new { id = postId });
            }
        }

        private bool IsAuthorized(string postOwnerId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");
            return isAdmin || postOwnerId == currentUserId;
        }
    }
}
