using System.Threading.Tasks;
using Fotoschachtel.ViewModels.Event;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Fotoschachtel.Controllers
{
    [Route("event")]
    public class EventController : Controller
    {
        [Route("")]
        public IActionResult Index()
        {
            return Redirect("~/");
        }

        [Route("{eventId}")]
        public async Task<IActionResult> Index([CanBeNull] string eventId)
        {
            //try
            //{
                return View(await HttpContext.RequestServices.GetService<IndexViewModel>().Fill(eventId));
            //}
            //catch
            //{
            //    return RedirectToAction("Index", "Home");
            //}
        }
    }
}