// Areas/ViewModels/UserProfileViewModel.cs
using GymOCommunity.Models;
using Microsoft.AspNetCore.Identity;

namespace GymOCommunity.ViewModels
{
    public class UserProfileViewModel
    {
        public IdentityUser User { get; set; }
        public List<Post> Posts { get; set; }
    }
}