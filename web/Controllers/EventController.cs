using System;
using System.Linq;
using System.Threading.Tasks;
using Fotoschachtel.Services;
using Fotoschachtel.ViewModels.Event;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace Fotoschachtel.Controllers
{
    [Route("")]
    public class EventController : Controller
    {
        private readonly MetadataService _metadataService;
        private readonly SasService _sasService;
        private readonly ThumbnailsService _thumbnailsService;

        public EventController(
            [NotNull] MetadataService metadataService,
            [NotNull] SasService sasService,
            [NotNull] ThumbnailsService thumbnailsService)
        {
            _metadataService = metadataService;
            _sasService = sasService;
            _thumbnailsService = thumbnailsService;
        }


        [Route("event/{eventId}")]
        public async Task<IActionResult> Index([NotNull] string eventId)
        {
            return await Index(eventId, "");
        }


        [Route("event/{eventId}:{password}")]
        public async Task<IActionResult> Index([NotNull] string eventId, [NotNull] string password)
        {
            var metadata = await _metadataService.GetOrLoad();
            var eventMetadata = metadata.Events.FirstOrDefault(x => x.Event.Equals(eventId, StringComparison.InvariantCultureIgnoreCase));
            if (eventMetadata == null)
            {
                return View("NotFound", new NotFoundViewModel
                {
                    Event = eventId
                });
            }
            if (eventMetadata.Password != password)
            {
                return View("Unauthorized", new UnauthorizedViewModel
                {
                    Event = eventMetadata.Event
                });
            }

            // make sure all thumbnails are up-to-date
            await _thumbnailsService.RenderThumbnails(eventMetadata.ContainerName);

            var viewModel = new IndexViewModel(_sasService);
            return View(await viewModel.Fill(eventMetadata.Event, eventMetadata.ContainerName));
        }


        [Route("create-event")]
        public IActionResult Create([CanBeNull] string @event)
        {
            var viewModel = new CreateViewModel { Event = @event };
            return View(viewModel);
        }


        [Route("create-event")]
        [HttpPost]
        public async Task<IActionResult> Create([NotNull] CreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var metadata = await _metadataService.GetOrLoad();
                if (metadata.Events.Any(x => x.Event.Equals(viewModel.Event)))
                {
                    ModelState.AddModelError("event", "Es gibt bereits ein Event mit diesem Namen.");
                }
                else
                {
                    var eventMetadata = new EventMetadata
                    {
                        Event = viewModel.Event,
                        Password = viewModel.Password,
                        CreatedDateTime = DateTime.UtcNow,
                        CreatorEmail = viewModel.Email,
                        ContainerName = SasService.GetContainerName(viewModel.Event)
                    };
                    metadata.Events = metadata.Events.Concat(new[] { eventMetadata }).ToArray();
                    await _metadataService.Save(metadata);

                    return RedirectToAction("Index", new { eventId = viewModel.Event, password = viewModel.Password });
                }
            }
            return View(viewModel);
        }
    }
}