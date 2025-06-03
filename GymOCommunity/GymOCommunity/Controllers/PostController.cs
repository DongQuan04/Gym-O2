using GymOCommunity.Data;
using GymOCommunity.Models;
using GymOCommunity.Models.ViewModels;
using GymOCommunity.Services;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
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
        private readonly INotificationService _notificationService;

        public PostsController(
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<PostsController> logger,
            INotificationService notificationService)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _notificationService = notificationService;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var posts = await _context.Posts
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PostViewModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    ImageUrl = p.ImageUrl,
                    CreatedAt = p.CreatedAt,
                    UserId = p.UserId,
                    UserName = _context.Users
                        .Where(u => u.Id == p.UserId)
                        .Select(u => u.UserName)
                        .FirstOrDefault() ?? "Ẩn danh"
                })
                .ToListAsync();

            return View(new PostListViewModel { Posts = posts });
        }


        [AllowAnonymous]
        public IActionResult Details(int id)
        {
            var post = _context.Posts
                .Include(p => p.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .Include(p => p.PostImages)
                .Include(p => p.PostVideos)
                .FirstOrDefault(p => p.Id == id);

            if (post == null)
                return NotFound();

            return View(post);
        }

        [HttpGet]
        public async Task<IActionResult> Share(int id)
        {
            var post = await _context.Posts
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
                return NotFound();

            var user = await _userManager.FindByIdAsync(post.UserId);
            var userName = user?.UserName ?? "Ẩn danh";

            var model = new SharePostViewModel
            {
                OriginalPostId = post.Id,
                Title = post.Title,
                Description = post.Description,
                ImageUrl = post.ImageUrl,
                AuthorName = userName,
                CreatedAt = post.CreatedAt
            };

            return View("ShareForm", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Share(SharePostViewModel model)
        {
            if (!ModelState.IsValid)
            {
                foreach (var err in ModelState)
                {
                    Console.WriteLine($"{err.Key}: {string.Join(", ", err.Value.Errors.Select(e => e.ErrorMessage))}");
                }
                return View("ShareForm", model);
            }

            var post = await _context.Posts.FindAsync(model.OriginalPostId);
            if (post == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            var sharedPost = new SharedPost
            {
                OriginalPostId = model.OriginalPostId,
                UserId = currentUserId,
                SharedAt = DateTime.UtcNow,
                Note = model.Note
            };

            _context.SharedPosts.Add(sharedPost);
            await _context.SaveChangesAsync();

            // Gửi thông báo khi share bài viết (trừ trường hợp tự share)
            if (post.UserId != currentUserId)
            {
                await _notificationService.CreateNotification(
                    userId: post.UserId,
                    triggerUserId: currentUserId,
                    type: NotificationType.Share,
                    postId: post.Id,
                    message: $"đã chia sẻ bài viết của bạn: {post.Title.Truncate(30)}");
            }

            return RedirectToAction("Index", "Profile", new { userId = sharedPost.UserId });
        }

        public IActionResult Create()
        {
            return View();
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
            await _context.SaveChangesAsync();

            // Xử lý nhiều ảnh bổ sung
            var additionalImages = Request.Form.Files.Where(f => f.Name == "AdditionalImages");
            foreach (var image in additionalImages)
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

            // Xử lý nhiều video
            var videoFiles = Request.Form.Files.Where(f => f.Name == "VideoFiles");
            foreach (var video in videoFiles)
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

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Profile", new { userId = post.UserId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LikeComment(int id)
        {
            var comment = await _context.Comments
                .Include(c => c.Post)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null) return NotFound();

            comment.Likes++;
            await _context.SaveChangesAsync();

            // Gửi thông báo khi like comment (trừ trường hợp tự like)
            if (comment.UserId != _userManager.GetUserId(User))
            {
                await _notificationService.CreateNotification(
                    userId: comment.UserId,
                    triggerUserId: _userManager.GetUserId(User),
                    type: NotificationType.Like,
                    postId: comment.PostId,
                    message: $"đã thích bình luận của bạn: {comment.Content.Truncate(30)}");
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { newLikeCount = comment.Likes });
            }

            return RedirectToAction("Details", "Posts", new { id = comment.PostId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(100 * 1024 * 1024)] // 100MB
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

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var comment = new Comment
                {
                    Content = content,
                    PostId = postId,
                    ParentCommentId = parentCommentId,
                    UserId = currentUserId,
                    UserName = User.Identity?.Name,
                    CreatedAt = DateTime.Now
                };

                // Xử lý upload video
                if (videoFile != null && videoFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "comment_videos");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(videoFile.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await videoFile.CopyToAsync(fileStream);
                    }

                    comment.VideoUrl = $"/uploads/comment_videos/{uniqueFileName}";
                }

                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();

                // Gửi thông báo khi comment (trừ trường hợp tự comment)
                if (post.UserId != currentUserId)
                {
                    await _notificationService.CreateNotification(
                        userId: post.UserId,
                        triggerUserId: currentUserId,
                        type: NotificationType.Comment,
                        postId: postId,
                        message: $"đã bình luận về bài viết của bạn: {content.Truncate(30)}");
                }

                // Nếu là reply comment, gửi thông báo cho chủ comment gốc
                if (parentCommentId.HasValue)
                {
                    var parentComment = await _context.Comments
                        .FirstOrDefaultAsync(c => c.Id == parentCommentId.Value);

                    if (parentComment != null && parentComment.UserId != currentUserId)
                    {
                        await _notificationService.CreateNotification(
                            userId: parentComment.UserId,
                            triggerUserId: currentUserId,
                            type: NotificationType.Comment,
                            postId: postId,
                            message: $"đã trả lời bình luận của bạn: {content.Truncate(30)}");
                    }
                }

                return RedirectToAction("Details", new { id = postId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm bình luận có video");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi gửi bình luận";
                return RedirectToAction("Details", new { id = postId });
            }
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

        private bool IsAuthorized(string postOwnerId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");
            return isAdmin || postOwnerId == currentUserId;
        }
    }

    public static class StringExtensions
    {
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "...";
        }
    }

    public static class FormFileExtensions
    {
        public static IEnumerable<IFormFile> Where(this IFormFileCollection formFiles, Func<IFormFile, bool> predicate)
        {
            foreach (var file in formFiles)
            {
                if (predicate(file))
                {
                    yield return file;
                }
            }
        }
    }
}