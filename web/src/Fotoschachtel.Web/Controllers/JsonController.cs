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
        private readonly HashService _hashService;

        public JsonController(
            [NotNull] MetadataService metadataService,
            [NotNull] SasService sasService,
            [NotNull] ThumbnailsService thumbnailsService,
            [NotNull] HashService hashService)
        {
            _metadataService = metadataService;
            _sasService = sasService;
            _thumbnailsService = thumbnailsService;
            _hashService = hashService;
        }


        [HttpGet]
        public async Task<IActionResult> GetStorageUrl([NotNull] string @event, [CanBeNull] string password)
        {
            var metadata = await _metadataService.GetOrLoad();
            var eventMetadata = metadata.Events.FirstOrDefault(x => x.Event.Equals(@event, StringComparison.OrdinalIgnoreCase));
            if (eventMetadata == null)
            {
                return new NotFoundResult();
            }
            if (eventMetadata.Password != (password ?? "") && _hashService.HashEventPassword(eventMetadata.Event, eventMetadata.Password) != password)
            {
                return new UnauthorizedResult();
            }
            return new JsonResult(await _sasService.GetSasForContainer(eventMetadata.Event, eventMetadata.ContainerName));
        }


        [HttpPost]
        public async Task<IActionResult> RenderThumbnails([NotNull] string @event, [CanBeNull] string password)
        {
            var metadata = await _metadataService.GetOrLoad();
            var eventMetadata = metadata.Events.FirstOrDefault(x => x.Event.Equals(@event, StringComparison.OrdinalIgnoreCase));
            if (eventMetadata == null)
            {
                return new NotFoundResult();
            }
            if (eventMetadata.Password != (password ?? "") && _hashService.HashEventPassword(eventMetadata.Event, eventMetadata.Password) != password)
            {
                return new UnauthorizedResult();
            }
            await _thumbnailsService.RenderThumbnails(eventMetadata.ContainerName);
            return new NoContentResult();
        }

    }
}
