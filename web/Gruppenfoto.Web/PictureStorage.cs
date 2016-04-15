using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ImageProcessor.Imaging;
using ImageProcessor.Imaging.Formats;
using JetBrains.Annotations;
using Microsoft.Extensions.PlatformAbstractions;
using System.Drawing;

namespace Gruppenfoto.Web
{
    public class PictureStorage
    {
        [NotNull]
        private readonly string _rootPath;

        public PictureStorage([NotNull] IApplicationEnvironment appEnv)
        {
            if (!string.IsNullOrWhiteSpace(appEnv.ApplicationBasePath))
            {
                _rootPath = Path.Combine(appEnv.ApplicationBasePath, "App_Data");
            }
            else
            {
                throw new Exception("ApplicationBasePath is null.");
            }
        }


        [NotNull]
        public Picture Save([NotNull] string eventId, [NotNull] byte[] content)
        {
            var fileId = Guid.NewGuid().ToString();
            var filePath = Path.Combine(GetEventDirectory(eventId), fileId + ".jpg");
            File.WriteAllBytes(filePath, content);
            return new Picture(eventId, fileId, DateTime.UtcNow);
        }


        [NotNull]
        public IEnumerable<Picture> Load([NotNull] string eventId)
        {
            var eventDirectory = GetEventDirectory(eventId);
            return new DirectoryInfo(eventDirectory)
                .GetFiles("*.jpg")
                .OrderBy(x => x.CreationTimeUtc)
                .Select(f => new Picture(eventId, Path.GetFileNameWithoutExtension(f.Name), f.CreationTimeUtc))
                .ToList();
        }


        [NotNull]
        public Stream GetStream([NotNull] string eventId, string fileId, int? size)
        {
            var eventDirectory = GetEventDirectory(eventId);
            var filePath = Path.Combine(eventDirectory, fileId + ".jpg");
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Picture not found", filePath);
            }

            if (size.HasValue)
            {
                using (var inStream = new FileStream(filePath, FileMode.Open))
                {
                    var outStream = new MemoryStream();
                    using (var imageFactory = new ImageProcessor.ImageFactory())
                    {
                        imageFactory
                            .Load(inStream)
                            .AutoRotate()
                            .Resize(new ResizeLayer(new Size(size.Value, size.Value), ResizeMode.Max))
                            .Format(new JpegFormat())
                            .Quality(90)
                            .Save(outStream);
                    }
                    return outStream;
                }
            }

            return new FileStream(filePath, FileMode.Open);
        }


        [NotNull]
        private string GetEventDirectory([NotNull] string eventId)
        {
            var eventDirectory = Path.Combine(_rootPath, eventId);
            if (string.IsNullOrWhiteSpace(eventDirectory))
            {
                throw new Exception($"Unable to get path for event directory for event {eventId}.");
            }

            if (!Directory.Exists(eventDirectory))
            {
                Directory.CreateDirectory(eventDirectory);
            }

            return eventDirectory;
        }
    }

    public class Picture
    {
        public Picture([NotNull]string eventId, [NotNull]string fileId, DateTime creationDateTime)
        {
            EventId = eventId;
            FileId = fileId;
            CreationDateTime = creationDateTime;
        }

        public string EventId { get; set; }
        public string FileId { get; set; }
        public DateTime CreationDateTime { get; set; }
    }
}
