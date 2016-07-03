using System;
using Microsoft.AspNetCore.Mvc;

namespace Fotoschachtel.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult TestErrorHandling()
        {
            throw new Exception("This exception is just a test.");
        }
    }
}
