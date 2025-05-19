using System;
using System.ComponentModel.DataAnnotations;

namespace GymOCommunity.Models.ViewModels
{
    public class SharePostViewModel
    {
        [Required]
        public int OriginalPostId { get; set; } // ID của bài viết gốc
        
        // Dữ liệu hiển thị
        public string Title { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public string AuthorName { get; set; } = "Ẩn danh";

        [Required(ErrorMessage = "Vui lòng nhập thông điệp chia sẻ")]
        public string Note { get; set; }
        public string? Description { get; set; }

    }
}
