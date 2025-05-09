using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace GymOCommunity.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Content { get; set; }

        public string? VideoUrl { get; set; } // Thêm trường video

        [NotMapped]
        public IFormFile? VideoFile { get; set; } // File video upload


        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Quan hệ cha-con
        public int? ParentCommentId { get; set; }
        public virtual Comment? ParentComment { get; set; }
        public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();

        // Khóa ngoại liên kết với bài viết
        [ForeignKey("Post")]
        public int PostId { get; set; }
        public virtual Post Post { get; set; }

        // Khóa ngoại liên kết với người dùng
        [ForeignKey("User")]
        public string UserId { get; set; }

        public virtual IdentityUser User { get; set; }  // Quan hệ đúng kiểu

        [Required]
        public string UserName { get; set; }

        public int Likes { get; set; } = 0;


    }
}
