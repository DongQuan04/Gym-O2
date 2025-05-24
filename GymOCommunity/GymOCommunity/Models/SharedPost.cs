using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace GymOCommunity.Models
{
    public class SharedPost
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OriginalPostId { get; set; }

        [ForeignKey("OriginalPostId")]
        public Post OriginalPost { get; set; }

        [Required]
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public IdentityUser User { get; set; }

        public DateTime SharedAt { get; set; } 

        public string? Note { get; set; } // Ghi chú khi chia sẻ lại
    }
}
