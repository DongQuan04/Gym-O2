using GymOCommunity.Data;
using GymOCommunity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace GymOCommunity.Controllers
{
   
    public class CommentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CommentController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Create(int postId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return BadRequest("Nội dung bình luận không được để trống.");
            }

            var userName = User.Identity.IsAuthenticated ? User.Identity.Name : "Anonymous"; // Lấy tên user
            var userId = User.Identity.Name; // Giữ nguyên UserId từ Identity

            var comment = new Comment
            {
                PostId = postId,
                Content = content,
                UserId = userId,
                UserName = userName // tránh lỗi thiếu dữ liệu
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Post", new { id = postId });
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LikeComment(int commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
            {
                return NotFound();
            }

            comment.Likes++;
            await _context.SaveChangesAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = true,
                    newLikeCount = comment.Likes
                });
            }

            // Fallback cho trình duyệt không hỗ trợ JavaScript
            return RedirectToAction("Details", "Posts", new { id = comment.PostId });
        }

    }
}
