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

            // Bài viết gốc
            var posts = await _context.Posts
                .Include(p => p.PostImages)
                .Include(p => p.PostVideos)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            // Bài viết chia sẻ
            var sharedPosts = await _context.SharedPosts
                .Include(sp => sp.OriginalPost)
                    .ThenInclude(p => p.PostImages)
                .Include(sp => sp.OriginalPost)
                    .ThenInclude(p => p.PostVideos)
                .Where(sp => sp.UserId == userId)
                .OrderByDescending(sp => sp.SharedAt)
                .ToListAsync();

            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.UserId == userId);

            var viewModel = new UserProfileViewModel
            {
                User = user,
                Email = user.Email,
                Posts = posts,
                SharedPosts = sharedPosts, // 👈 Thêm vào đây
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

                // Cập nhật các trường còn lại
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
