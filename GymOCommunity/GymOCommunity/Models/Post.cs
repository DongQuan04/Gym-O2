using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Identity;

namespace GymOCommunity.Models
{
    public class Post
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Nội dung không được để trống")]
        public string Content { get; set; }

        public string? ImageUrl { get; set; } // Đường dẫn ảnh 

        [NotMapped] 
      
        public IFormFile? ImageFile { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int? ViewCount { get; set; } = 0;

        public bool IsFeatured { get; set; } = false;


        public string? UserId { get; set; }

        public List<Comment> Comments { get; set; } = new List<Comment>();

        public List<PostImage> Images { get; set; } = new List<PostImage>();

        [NotMapped]
        public List<IFormFile> AdditionalImages { get; set; } = new List<IFormFile>();// Upload nhiều ảnh

        public virtual ICollection<PostImage> PostImages { get; set; } = new List<PostImage>();

        public string? VideoUrl { get; set; }
        [NotMapped]
        public IFormFile? VideoFile { get; set; }

        public List<PostVideo> PostVideos { get; set; } = new List<PostVideo>();

        [NotMapped]
        public List<IFormFile> VideoFiles { get; set; } = new List<IFormFile>();

        public string? Description { get; set; }

        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        [ForeignKey("UserId")]
        public virtual IdentityUser? User { get; set; }

    }
}
