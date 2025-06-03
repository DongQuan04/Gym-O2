using GymOCommunity.Data;
using GymOCommunity.Models;
using GymOCommunity.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GymOCommunity.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ProfileController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            // Load bài viết gốc + hình ảnh/video đính kèm
            var posts = await _context.Posts
                .Include(p => p.PostImages)
                .Include(p => p.PostVideos)
                .Include(p => p.User)  // Load thông tin người đăng
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            // Load bài viết đã chia sẻ
            var sharedPosts = await _context.SharedPosts
                .Include(sp => sp.OriginalPost)
                    .ThenInclude(p => p.PostImages)
                .Include(sp => sp.OriginalPost)
                    .ThenInclude(p => p.PostVideos)
                .Include(sp => sp.OriginalPost)
                    .ThenInclude(p => p.User)  // Load thông tin người đăng bài gốc
                .Where(sp => sp.UserId == userId)
                .OrderByDescending(sp => sp.SharedAt)
                .ToListAsync();

            // Load thông tin profile
            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.UserId == userId);

            // Tạo ViewModel
            var viewModel = new UserProfileViewModel
            {
                User = user,
                Email = user.Email,
                Posts = posts ?? new List<Post>(),  // Đảm bảo không null
                SharedPosts = sharedPosts ?? new List<SharedPost>(),  // Đảm bảo không null
                FullName = userProfile?.FullName ?? user.UserName,
                Bio = userProfile?.Bio,
                AvatarUrl = userProfile?.AvatarUrl,
                Location = userProfile?.Location,
                Website = userProfile?.Website
            };

            return View(viewModel);
        }


        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.UserId == user.Id);

            var viewModel = new ProfileEditViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FullName = userProfile?.FullName ?? user.UserName,
                Bio = userProfile?.Bio,
                AvatarUrl = userProfile?.AvatarUrl,
                Location = userProfile?.Location,
                Website = userProfile?.Website
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null || user.Id != model.Id)
                {
                    return NotFound();
                }

                user.Email = model.Email;
                user.UserName = model.Email;

                var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.UserId == user.Id)
                                  ?? new UserProfile { UserId = user.Id };

                // Xử lý xóa avatar cũ nếu có
                if (model.AvatarFile != null && model.AvatarFile.Length > 0)
                {
                    if (!string.IsNullOrEmpty(userProfile.AvatarUrl))
                    {
                        var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", userProfile.AvatarUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = $"{user.Id}_{Guid.NewGuid()}{Path.GetExtension(model.AvatarFile.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.AvatarFile.CopyToAsync(fileStream);
                    }

                    userProfile.AvatarUrl = $"/uploads/avatars/{uniqueFileName}";
                }

                // Cập nhật các trường
                userProfile.FullName = model.FullName;
                userProfile.Bio = model.Bio;
                userProfile.Location = model.Location;
                userProfile.Website = model.Website;

                if (userProfile.Id == 0)
                    _context.UserProfiles.Add(userProfile);
                else
                    _context.UserProfiles.Update(userProfile);

                var userResult = await _userManager.UpdateAsync(user);
                if (!userResult.Succeeded)
                {
                    foreach (var error in userResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                return RedirectToAction(nameof(Index), new { userId = user.Id });
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi cập nhật thông tin.");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAvatar()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.UserId == user.Id);

            if (userProfile != null && !string.IsNullOrEmpty(userProfile.AvatarUrl))
            {
                var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", userProfile.AvatarUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }

                userProfile.AvatarUrl = null;
                _context.UserProfiles.Update(userProfile);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Edit));
        }
    }
}
