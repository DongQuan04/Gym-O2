using System;
using System.ComponentModel.DataAnnotations;

namespace GymOCommunity.Models
{
    public class Post
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Nội dung không được để trống")]
        public string Content { get; set; }

        [Required(ErrorMessage = "Ảnh không được để trống")]
        public string ImageUrl { get; set; }  // 

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}