using Microsoft.AspNetCore.Mvc;

namespace ProjectCookBook.Controllers
{
    public class RecettesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
