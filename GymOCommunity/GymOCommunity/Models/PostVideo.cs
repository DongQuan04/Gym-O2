namespace GymOCommunity.Models
{
    public class PostVideo
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public string VideoUrl { get; set; }

        public Post Post { get; set; }
    }
}
