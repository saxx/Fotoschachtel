using System;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fotoschachtel.Services;
using Fotoschachtel.ViewModels.Event;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using ZXing;
using ZXing.Common;

namespace Fotoschachtel.Controllers
{
    [Route("")]
    public class EventController : Controller
    {
        #region Constructor

        private readonly MetadataService _metadataService;
        private readonly SasService _sasService;
        private readonly ThumbnailsService _thumbnailsService;
        private readonly HashService _hashService;

        public EventController(
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

        #endregion


        #region Index
        [Route("event/{event}")]
        public async Task<IActionResult> Index([NotNull] string @event)
        {
            return await Index(@event, "");
        }


        [Route("event/{event}:{password}")]
        public async Task<IActionResult> Index([NotNull] string @event, [NotNull] string password)
        {
            var metadata = await _metadataService.GetOrLoad();
            var eventMetadata = metadata.Events.FirstOrDefault(x => x.Event.Equals(@event, StringComparison.InvariantCultureIgnoreCase));
            if (eventMetadata == null)
            {
                return View("NotFound", new NotFoundViewModel
                {
                    Event = @event
                });
            }
            if (_hashService.HashEventPassword(@event, eventMetadata.Password) != password)
            {
                return View("Unauthorized", new UnauthorizedViewModel
                {
                    Event = eventMetadata.Event
                });
            }

            // make sure all thumbnails are up-to-date
            await _thumbnailsService.RenderThumbnails(eventMetadata.ContainerName);
            return View(await new IndexViewModel(_sasService, _hashService).Fill(eventMetadata));
        }
        #endregion


        #region Code
        [Route("event/{event}/code")]
        public async Task<IActionResult> Code([NotNull] string @event)
        {
            return await Index(@event, "");
        }


        [Route("event/{event}:{password}/code")]
        public async Task<IActionResult> Code([NotNull] string @event, [NotNull] string password)
        {
            var metadata = await _metadataService.GetOrLoad();
            var eventMetadata = metadata.Events.FirstOrDefault(x => x.Event.Equals(@event, StringComparison.InvariantCultureIgnoreCase));
            if (eventMetadata == null)
            {
                return NotFound();
            }
            if (_hashService.HashEventPassword(@event, eventMetadata.Password) != password)
            {
                return Unauthorized();
            }

            var barcodeWriter = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new EncodingOptions
                {
                    Height = 250,
                    Width = 250,
                    Margin = 1
                }
            };

            using (var bitmap = barcodeWriter.Write($"{eventMetadata.Event}:{eventMetadata.Password}"))
            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Png);
                return new FileContentResult(stream.ToArray(), "image/png");
            }
        }
        #endregion


        #region Poster
        [Route("event/{event}/poster")]
        public async Task<IActionResult> Poster([NotNull] string @event)
        {
            return await Index(@event, "");
        }


        [Route("event/{event}:{password}/poster")]
        public async Task<IActionResult> Poster([NotNull] string @event, [NotNull] string password)
        {
            var metadata = await _metadataService.GetOrLoad();
            var eventMetadata = metadata.Events.FirstOrDefault(x => x.Event.Equals(@event, StringComparison.InvariantCultureIgnoreCase));
            if (eventMetadata == null)
            {
                return View("NotFound", new NotFoundViewModel
                {
                    Event = @event
                });
            }
            if (_hashService.HashEventPassword(@event, eventMetadata.Password) != password)
            {
                return View("Unauthorized", new UnauthorizedViewModel
                {
                    Event = eventMetadata.Event
                });
            }
            return View(await new IndexViewModel(_sasService, _hashService).Fill(eventMetadata));
        }
        #endregion


        #region Create
        [Route("create-event")]
        public IActionResult Create([CanBeNull] string @event)
        {
            var viewModel = new CreateViewModel { Event = @event };
            return View(viewModel);
        }


        [Route("create-event")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([NotNull] CreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var metadata = await _metadataService.GetOrLoad();
                if (metadata.Events.Any(x => x.Event.Equals(viewModel.Event, StringComparison.InvariantCultureIgnoreCase)))
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

                    return RedirectToAction("Index", new
                    {
                        @event = viewModel.Event,
                        password = _hashService.HashEventPassword(viewModel.Event, viewModel.Password)
                    });
                }
            }
            return View(viewModel);
        }
        #endregion


        [Route("authorize")]
        [HttpPost]
        public IActionResult Unauthorized([NotNull] string @event, [CanBeNull] string password)
        {
            var hashedPassword = _hashService.HashEventPassword(@event, password);

            return RedirectToAction("Index", new
            {
                @event,
                password = hashedPassword
            });
        }
    }
}