using Microsoft.AspNetCore.Mvc;

namespace Fotoschachtel.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
