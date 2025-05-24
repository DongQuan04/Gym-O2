using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using GymOCommunity.Models;

namespace GymOCommunity.Models
{
   
    public class Notification
    {
        public int Id { get; set; }
        public string? UserId { get; set; } // Người nhận
        public string TriggerUserId { get; set; } // Người khác thao tác
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public NotificationType Type { get; set; }
        public int? PostId { get; set; } // Liên kết với post

        // Navigation properties
        public virtual IdentityUser User { get; set; }
        public virtual IdentityUser TriggerUser { get; set; }
        public virtual Post Post { get; set; }
    }

    public enum NotificationType
    {
        Like,
        Comment,
        Share,
        Follow,
        Mention
    }
}
