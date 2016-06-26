using System;
using System.Net.Http;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Fotoschachtel.Services
{
    public class ThumbnailsService
    {
        public async Task RenderThumbnails([CanBeNull] string containerName)
        {
            if (string.IsNullOrWhiteSpace(containerName))
            {
                throw new ArgumentException("No container name id specified.");
            }
            containerName = SasService.GetContainerName(containerName);

            using (var client = new HttpClient())
            {
                await client.GetAsync($"https://fotoschachtel.azurewebsites.net/api/create-thumbnails?blob={containerName}");
            }
        }
    }
}
