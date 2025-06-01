using GymOCommunity.Models.ViewModels;

namespace GymOCommunity.Services
{
    public class NutritionService
    {
        public WeeklyMealPlanViewModel GenerateMealPlan(NutritionInputViewModel input)
        {
            // Kiểm tra nếu thiếu dữ liệu thì trả về null hoặc throw exception
            if (input.Age == null || input.Height == null || input.Weight == null ||
                string.IsNullOrEmpty(input.Gender) || string.IsNullOrEmpty(input.Goal) ||
                input.ActivityLevel == null)
            {
                return null; // hoặc throw new ArgumentException("Thiếu dữ liệu đầu vào");
            }

            double weight = input.Weight.Value;
            double height = input.Height.Value;
            int age = input.Age.Value;
            int activity = input.ActivityLevel.Value;

            double bmr = input.Gender == "Nam"
                ? 10 * weight + 6.25 * height - 5 * age + 5
                : 10 * weight + 6.25 * height - 5 * age - 161;

            double tdee = bmr * (1.2 + activity * 0.1);

            if (input.Goal == "Tăng cơ") tdee += 300;
            if (input.Goal == "Giảm mỡ") tdee -= 300;

            var plan = new WeeklyMealPlanViewModel
            {
                Calories = tdee,
                Protein = weight * 2.2,
                Carbs = (tdee * 0.5) / 4,
                Fat = (tdee * 0.2) / 9,
                Meals = new List<Meal>()
            };

            string[] days = { "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7", "Chủ nhật" };
            string[] meals = { "Sáng", "Trưa", "Tối" };
            string[] dishes = { "Ức gà + khoai", "Trứng + bánh mì", "Cơm gạo lứt + cá", "Cháo yến mạch + trứng", "Salad cá ngừ + trứng" };

            var rnd = new Random();
            foreach (var day in days)
            {
                foreach (var time in meals)
                {
                    plan.Meals.Add(new Meal
                    {
                        Day = day,
                        MealTime = time,
                        Dish = dishes[rnd.Next(dishes.Length)]
                    });
                }
            }

            return plan;
        }
    }
}
