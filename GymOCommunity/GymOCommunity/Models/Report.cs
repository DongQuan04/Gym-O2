using System;

namespace GymOCommunity.Models
{
    public class Report
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }

        public int PostId { get; set; }

        public DateTime ReportedAt { get; set; } 

        public Post? Post { get; set; }  // ← Navigation property cần thiết
    }
}
