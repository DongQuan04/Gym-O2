using GymOCommunity.Models.ViewModels;
using GymOCommunity.Services;
using Microsoft.AspNetCore.Mvc;

public class ExerciseController : Controller
{
    private readonly NutritionService _nutritionService;
    public IActionResult Index()
    {
        return View();
    }
    public ExerciseController()
    {
        _nutritionService = new NutritionService(); // Nếu dùng DI thì inject
    }

    [HttpGet]
    public IActionResult Nutrition()
    {
        return View(new NutritionInputViewModel());
    }

    [HttpPost]
    public IActionResult Nutrition(NutritionInputViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var mealPlan = _nutritionService.GenerateMealPlan(model);
        ViewBag.MealPlan = mealPlan;
        return View(model);
    }
}







