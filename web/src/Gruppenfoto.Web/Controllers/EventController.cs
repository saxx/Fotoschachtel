using System.Threading.Tasks;
using Gruppenfoto.Web.ViewModels.Event;
using JetBrains.Annotations;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace GruppenFoto.Web.Controllers
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
                return View(await HttpContext.ApplicationServices.GetRequiredService<IndexViewModel>().Fill(eventId));
            //}
            //catch
            //{
            //    return RedirectToAction("Index", "Home");
            //}
        }
    }
}