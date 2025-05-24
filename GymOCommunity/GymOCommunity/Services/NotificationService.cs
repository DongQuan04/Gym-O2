using GymOCommunity.Data;
using GymOCommunity.Hubs;
using GymOCommunity.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace GymOCommunity.Services
{
    // Interface định nghĩa các phương thức của NotificationService
    public interface INotificationService
    {
        Task CreateNotification(string userId, string triggerUserId, NotificationType type, int? postId = null, string message = null);
        Task<List<Notification>> GetUserNotifications(string userId);
        Task MarkAsRead(int notificationId);
    }

    // Cài đặt dịch vụ NotificationService
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(ApplicationDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task CreateNotification(string userId, string triggerUserId, NotificationType type, int? postId = null, string message = null)
        {
            var defaultMessage = type switch
            {
                NotificationType.Like => "đã thích bài viết của bạn",
                NotificationType.Comment => "đã bình luận về bài viết của bạn",
                NotificationType.Share => "đã chia sẻ bài viết của bạn",
                _ => "đã tương tác với bài viết của bạn"
            };

            var notification = new Notification
            {
                UserId = userId,
                TriggerUserId = triggerUserId,
                Type = type,
                PostId = postId,
                Message = message ?? defaultMessage,
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Gửi thông báo realtime tới client sau khi đã lưu
            await _hubContext.Clients.Group(userId).SendAsync("ReceiveNotification", new
            {
                Id = notification.Id,
                Message = notification.Message,
                PostId = notification.PostId,
                Type = notification.Type.ToString(),
                CreatedAt = notification.CreatedAt.ToString("HH:mm dd/MM")
            });
        }

        public async Task<List<Notification>> GetUserNotifications(string userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task MarkAsRead(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
