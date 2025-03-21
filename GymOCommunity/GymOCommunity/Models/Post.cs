using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace GymOCommunity.Models
{
    public class Post
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Nội dung không được để trống")]
        public string Content { get; set; }

        public string? ImageUrl { get; set; } // Đường dẫn ảnh trong server

        [NotMapped] // Không lưu vào DB
        [Required(ErrorMessage = "Ảnh không được để trống")]
        public IFormFile? ImageFile { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // 🔹 Danh sách bình luận (Thêm vào đây)
        public List<Comment> Comments { get; set; } = new List<Comment>();
    }
}
