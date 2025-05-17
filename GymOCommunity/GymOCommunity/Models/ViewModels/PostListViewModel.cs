namespace GymOCommunity.Models.ViewModels
{
    using System.Collections.Generic;
    using GymOCommunity.Models;

    public class PostListViewModel
    {
        public List<PostViewModel> Posts { get; set; } = new();
    }
}
