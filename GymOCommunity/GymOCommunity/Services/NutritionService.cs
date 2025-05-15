using GymOCommunity.Models.ViewModels;

namespace GymOCommunity.Services
{
    public class NutritionService
    {
        public WeeklyMealPlanViewModel GenerateMealPlan(NutritionInputViewModel input)
        {
            // Giả lập: dùng công thức đơn giản để tính TDEE
            double bmr = input.Gender == "Nam"
                ? 10 * input.Weight + 6.25 * input.Height - 5 * input.Age + 5
                : 10 * input.Weight + 6.25 * input.Height - 5 * input.Age - 161;

            double tdee = bmr * (1.2 + input.ActivityLevel * 0.1);

            if (input.Goal == "Tăng cơ") tdee += 300;
            if (input.Goal == "Giảm mỡ") tdee -= 300;

            var plan = new WeeklyMealPlanViewModel
            {
                Calories = tdee,
                Protein = input.Weight * 2.2,
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
