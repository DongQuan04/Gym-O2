namespace GymOCommunity.Models.ViewModels
{
    public class NutritionInputViewModel
    {
        public int? Age { get; set; }

        public double? Height { get; set; }

        public double? Weight { get; set; }

        public string Gender { get; set; }

        public string Goal { get; set; }

        public int? ActivityLevel { get; set; } // số buổi tập mỗi tuần
    }
}
