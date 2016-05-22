using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace Fotoschachtel.Controllers
{
    [Route("json")]
    public class JsonController
    {
        private readonly SasService _sasService;
        private readonly ThumbnailsService _thumbnailsService;

        public JsonController([NotNull] SasService sasService, [NotNull] ThumbnailsService thumbnailsService)
        {
            _sasService = sasService;
            _thumbnailsService = thumbnailsService;
        }

        [HttpGet, Route("event/{eventId}")]
        public async Task<IActionResult> GetStorageUrl([CanBeNull] string eventId)
        {
            try
            {
                return new JsonResult(await _sasService.GetSasForEvent(eventId));
            }
            catch
            {
                return new BadRequestResult();
            }
        }


        [HttpPost, Route("event/{eventId}/picture/{filename}/thumbnails")]
        public async Task<IActionResult> RenderThumbnail([CanBeNull] string eventId, [CanBeNull] string filename)
        {
            await _thumbnailsService.RenderThumbnail(eventId, filename);
            return new NoContentResult();
        }


        [HttpPost, Route("event/{eventId}/thumbnails")]
        public async Task<IActionResult> RenderThumbnails([CanBeNull] string eventId, [CanBeNull] string filename)
        {
            await _thumbnailsService.RenderThumbnails(eventId);
            return new NoContentResult();
        }
    }
}
