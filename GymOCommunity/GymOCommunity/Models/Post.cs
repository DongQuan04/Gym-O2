using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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
      
        public IFormFile? ImageFile { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;


        public string? UserId { get; set; }


        // 🔹 Danh sách bình luận (Thêm vào đây)
        public List<Comment> Comments { get; set; } = new List<Comment>();
    }
}
