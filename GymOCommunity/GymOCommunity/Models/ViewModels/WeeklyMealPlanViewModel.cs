namespace GymOCommunity.Models.ViewModels
{
    public class Meal
    {
        public string Day { get; set; }

        public string MealTime { get; set; } // "Sáng", "Trưa", "Tối"

        public string Dish { get; set; }
    }

    public class WeeklyMealPlanViewModel
    {
        public List<Meal> Meals { get; set; }

        public double Calories { get; set; }

        public double Protein { get; set; }

        public double Carbs { get; set; }

        public double Fat { get; set; }
    }
}
