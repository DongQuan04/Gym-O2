namespace GymOCommunity.Models
{
    public class PostImage
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = null!;

        // Khóa ngoại
        public int PostId { get; set; }
        public Post Post { get; set; } = null!;
    }

}
