using System;
using System.IO;
using System.Threading.Tasks;
using Gruppenfoto.Web;
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
        public IActionResult Index([NotNull] string eventId)
        {
            var viewModel = HttpContext?.ApplicationServices.GetRequiredService<IndexViewModel>();
            if (viewModel == null)
            {
                throw new Exception("Unable to resolve IndexViewModel.");
            }
            return View(viewModel.Fill(eventId));
        }


        [HttpGet, Route("{eventId}/picture/{fileId}")]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
        public IActionResult DownloadPicture([NotNull] string eventId, [NotNull] string fileId, int? size)
        {
            var storage = HttpContext?.ApplicationServices.GetRequiredService<PictureStorage>();
            if (storage == null)
            {
                throw new Exception("Unable to resolve PictureStorage.");
            }
            return new FileStreamResult(storage.GetStream(eventId, fileId, size), "image/jpg");
        }


        [HttpPost, Route("{eventId}/picture")]
        public async Task<IActionResult> UploadPicture([NotNull] string eventId, [CanBeNull] UploadPictureViewModel viewModel)
        {
            if (viewModel == null || string.IsNullOrWhiteSpace(eventId))
            {
                return Json(false);
            }

            var storage = HttpContext?.ApplicationServices.GetRequiredService<PictureStorage>();
            if (storage == null)
            {
                throw new Exception("Unable to resolve PictureStorage.");
            }
            const int maxFileSize = 25*1024*1024;


            if (ModelState != null && ModelState.IsValid && viewModel.File != null && viewModel.File.Length > 0 && viewModel.File.Length < maxFileSize)
            {
                using (var stream = new MemoryStream())
                {
                    // ReSharper disable once PossibleNullReferenceException
                    await viewModel.File.OpenReadStream().CopyToAsync(stream);
                    storage.Save(eventId, stream.ToArray());
                    return Json(true);
                }
            }

            if (ModelState != null && ModelState.IsValid && viewModel.FileBase64 != null)
            {
                var bytes = Convert.FromBase64String(viewModel.FileBase64);
                if (bytes.Length < maxFileSize)
                {
                    storage.Save(eventId, bytes);
                }
            }

            return Json(false);
        }


        //        [Required]
        //public IFormFile File { get; set; }

    }
}

/*            if (viewModel == null)
            {
                return new RedirectResult("~/");
            }

            if (_controller.ModelState.IsValid)
            {
                if (viewModel.File != null
                    && viewModel.File.Length > 0
                    && viewModel.File.Length <= 10 * 1024 * 1024)
                {
                    string fileName;
                    try
                    {
                        fileName = ContentDispositionHeaderValue.Parse(viewModel.File.ContentDisposition).FileName.Trim(' ', '"', '\'');
                    }
                    catch
                    {
                        throw new Exception("Unable to parse content disposition header for filename.");
                    }

                    if (!string.IsNullOrWhiteSpace(fileName))
                    {

                        return new JsonResult(true);
                    }
                }
            }
            return new JsonResult(false);*/
