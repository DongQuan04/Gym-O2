using GymOCommunity.Models;
using Microsoft.AspNetCore.Identity;

namespace GymOCommunity.ViewModels
{
    public class UserProfileViewModel
    {
        public IdentityUser User { get; set; }
        public List<Post> Posts { get; set; }
        public string Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string? AvatarUrl { get; set; }
        public IFormFile AvatarFile { get; set; } // Avatar
        public string Bio { get; set; }
        public string Website { get; set; }
        public string Location { get; set; }
        public List<SharedPost> SharedPosts { get; set; }

    }
}