using System;
using System.Linq;
using System.Threading.Tasks;
using Fotoschachtel.Services;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace Fotoschachtel.Controllers
{
    public class JsonController
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


        [HttpGet]
        public async Task<IActionResult> GetStorageUrl([NotNull] string eventId, [CanBeNull] string password)
        {
            var metadata = await _metadataService.GetOrLoad();
            var eventMetadata = metadata.Events.FirstOrDefault(x => x.Event.Equals(eventId, StringComparison.InvariantCultureIgnoreCase));
            if (eventMetadata == null)
            {
                return new NotFoundResult();
            }
            if (eventMetadata.Password != (password ?? ""))
            {
                return new UnauthorizedResult();
            }
            return new JsonResult(await _sasService.GetSasForContainer(eventMetadata.Event, eventMetadata.ContainerName));
        }


        [HttpPost]
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
                return new UnauthorizedResult();
            }
            await _thumbnailsService.RenderThumbnail(eventMetadata.ContainerName, filename);
            return new NoContentResult();
        }


        [HttpPost]
        public async Task<IActionResult> RenderThumbnails([NotNull] string eventId, [CanBeNull] string password)
        {
            var metadata = await _metadataService.GetOrLoad();
            var eventMetadata = metadata.Events.FirstOrDefault(x => x.Event.Equals(eventId, StringComparison.InvariantCultureIgnoreCase));
            if (eventMetadata == null)
            {
                return new NotFoundResult();
            }
            if (eventMetadata.Password != (password ?? ""))
            {
                return new UnauthorizedResult();
            }
            await _thumbnailsService.RenderThumbnails(eventMetadata.ContainerName);
            return new NoContentResult();
        }

    }
}
