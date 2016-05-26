using System;
using System.Linq;
using System.Threading.Tasks;
using Fotoschachtel.Services;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace Fotoschachtel.Controllers
{
    [Route("json")]
    public class JsonController : Controller
    {
        private readonly MetadataService _metadataService;
        private readonly SasService _sasService;
        private readonly ThumbnailsService _thumbnailsService;

        public JsonController(
            [NotNull] MetadataService metadataService,
            [NotNull] SasService sasService,
            [NotNull] ThumbnailsService thumbnailsService)
        {
            _metadataService = metadataService;
            _sasService = sasService;
            _thumbnailsService = thumbnailsService;
        }


        [HttpGet, Route("event/{eventId}")]
        public async Task<IActionResult> GetStorageUrl([NotNull] string eventId)
        {
            return await GetStorageUrl(eventId, "");
        }


        [HttpGet, Route("event/{eventId}:{password}")]
        public async Task<IActionResult> GetStorageUrl([NotNull] string eventId, [NotNull] string password)
        {
            var metadata = await _metadataService.GetOrLoad();
            var eventMetadata = metadata.Events.FirstOrDefault(x => x.Event.Equals(eventId, StringComparison.InvariantCultureIgnoreCase));
            if (eventMetadata == null)
            {
                return NotFound();
            }
            if (eventMetadata.Password != password)
            {
                return Unauthorized();
            }
            return new JsonResult(await _sasService.GetSasForContainer(eventMetadata.Event, eventMetadata.ContainerName));
        }


        [HttpPost, Route("event/{eventId}/picture/{filename}/thumbnails")]
        public async Task<IActionResult> RenderThumbnail([NotNull] string eventId, [NotNull] string filename)
        {
            return await RenderThumbnail(eventId, "", filename);
        }


        [HttpPost, Route("event/{eventId}:{password}/picture/{filename}/thumbnails")]
        public async Task<IActionResult> RenderThumbnail([NotNull] string eventId, [NotNull] string password, [NotNull] string filename)
        {
            var metadata = await _metadataService.GetOrLoad();
            var eventMetadata = metadata.Events.FirstOrDefault(x => x.Event.Equals(eventId, StringComparison.InvariantCultureIgnoreCase));
            if (eventMetadata == null)
            {
                return new NotFoundResult();
            }
            if (eventMetadata.Password != password)
            {
                return Unauthorized();
            }
            await _thumbnailsService.RenderThumbnail(eventMetadata.ContainerName, filename);
            return new NoContentResult();
        }

        [HttpPost, Route("event/{eventId}/thumbnails")]
        public async Task<IActionResult> RenderThumbnails([NotNull] string eventId)
        {
            return await RenderThumbnails(eventId, "");
        }


        [HttpPost, Route("event/{eventId}:{password}/thumbnails")]
        public async Task<IActionResult> RenderThumbnails([NotNull] string eventId, [NotNull] string password)
        {
            var metadata = await _metadataService.GetOrLoad();
            var eventMetadata = metadata.Events.FirstOrDefault(x => x.Event.Equals(eventId, StringComparison.InvariantCultureIgnoreCase));
            if (eventMetadata == null)
            {
                return new NotFoundResult();
            }
            if (eventMetadata.Password != password)
            {
                return Unauthorized();
            }
            await _thumbnailsService.RenderThumbnails(eventMetadata.ContainerName);
            return new NoContentResult();
        }
    }
}
