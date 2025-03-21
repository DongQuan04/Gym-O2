using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymOCommunity.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Khóa ngoại liên kết với bài viết
        [ForeignKey("Post")]
        public int PostId { get; set; }
        public virtual Post Post { get; set; }

        // Khóa ngoại liên kết với người dùng
        [ForeignKey("User")]
        public string UserId { get; set; }

        // 🔹 Thêm thuộc tính UserName để sửa lỗi
        [Required]
        public string UserName { get; set; }
    }
}
