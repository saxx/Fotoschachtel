using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageProcessor.Imaging;
using ImageProcessor.Imaging.Formats;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Fotoschachtel.Services
{
    public class ThumbnailsService
    {
        private readonly Settings _settings;

        public ThumbnailsService([NotNull] IOptions<Settings> settings)
        {
            _settings = settings.Value;
        }


        private CloudBlobContainer _container;

        private CloudBlobContainer GetContainer([NotNull] string eventId)
        {
            if (_container == null)
            {
                var storageAccount = new CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(_settings.AzureStorageContainer, _settings.AzureStorageKey), true);
                var storageClient = storageAccount.CreateCloudBlobClient();
                var containerName = SasService.GetContainerName(eventId);
                _container = storageClient.GetContainerReference(containerName);
            }
            return _container;
        }


        public async Task RenderThumbnails([CanBeNull] string containerName)
        {
            if (string.IsNullOrWhiteSpace(containerName))
            {
                throw new ArgumentException("No container name id specified.");
            }
            if (string.IsNullOrWhiteSpace(_settings.AzureStorageContainer) || string.IsNullOrWhiteSpace(_settings.AzureStorageKey))
            {
                throw new Exception("Azure storage configuration missing");
            }

            var container = GetContainer(containerName);
            if (!await container.ExistsAsync())
            {
                return;
            }

            var existingFiles = container.ListBlobs(useFlatBlobListing: true).OfType<CloudBlockBlob>().Select(b => b.Name).ToList();
            var existingPrimaryFiles = existingFiles.Where(x => !x.StartsWith("thumbnail")).ToList();
            foreach (var f in existingPrimaryFiles)
            {
                var smallThumbnailExists = existingFiles.Any(x => x == "thumbnails-small/" + f);
                var mediumThumbnailExists = existingFiles.Any(x => x == "thumbnails-medium/" + f);

                if (!smallThumbnailExists || !mediumThumbnailExists)
                {
                    await RenderThumbnail(containerName, f);
                }
            }
        }


        public async Task RenderThumbnail([CanBeNull] string containerName, [CanBeNull] string fileName)
        {
            if (string.IsNullOrWhiteSpace(containerName) || string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("No container name or file name specified.");
            }
            if (string.IsNullOrWhiteSpace(_settings.AzureStorageContainer) || string.IsNullOrWhiteSpace(_settings.AzureStorageKey))
            {
                throw new Exception("Azure storage configuration missing");
            }

            var container = GetContainer(containerName);
            if (!await container.ExistsAsync())
            {
                return;
            }

            var sourceBlob = container.GetBlockBlobReference(fileName);

            var tempFilePath = Path.GetTempFileName();
            using (var inStream = new FileStream(tempFilePath, FileMode.Create))
            {
                await sourceBlob.DownloadToStreamAsync(inStream);
            }

            await RenderThumbnail(container, tempFilePath, 200, "thumbnails-small/" + fileName);
            await RenderThumbnail(container, tempFilePath, 1000, "thumbnails-medium/" + fileName);

            File.Delete(tempFilePath);
        }


        private async Task RenderThumbnail([NotNull] CloudBlobContainer container, [NotNull] string tempFilePath, int size, string targetFileName)
        {
            var targetBlob = container.GetBlockBlobReference(targetFileName);
            using (var targetStream = await targetBlob.OpenWriteAsync())
            {
                using (var imageFactory = new ImageProcessor.ImageFactory())
                {
                    imageFactory
                        .Load(tempFilePath)
                        .AutoRotate()
                        .Resize(new ResizeLayer(new Size(size, size), ResizeMode.Max))
                        .Format(new JpegFormat())
                        .Quality(90)
                        .Save(targetStream);
                }
            }
        }
    }
}
