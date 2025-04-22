using Microsoft.AspNetCore.Mvc;

namespace GymOCommunity.Controllers
{
    public class ExerciseController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
