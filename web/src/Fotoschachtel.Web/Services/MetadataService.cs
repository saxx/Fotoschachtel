using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace Fotoschachtel.Services
{
    public class MetadataService
    {
        private readonly Settings _settings;

        public MetadataService([NotNull] IOptions<Settings> settings)
        {
            _settings = settings.Value;
        }



        public async Task<Metadata> Load()
        {
            var container = await GetMetadataContainer();
            var file = container.GetBlockBlobReference("metadata.json");

            if (!await file.ExistsAsync())
            {
                return new Metadata
                {
                    Events = new[]
                    {
                        new EventMetadata
                        {
                            ContainerName = "sandbox",
                            CreatedDateTime = new DateTime(2016, 1, 1),
                            CreatorEmail = "hannes@sachsenhofer.com",
                            Event = "Sandbox",
                            Password = ""
                        },
                        new EventMetadata
                        {
                            ContainerName = "screenshots",
                            CreatedDateTime = new DateTime(2016, 1, 1),
                            CreatorEmail = "hannes@sachsenhofer.com",
                            Event = "Screenshots",
                            Password = ""
                        },
                        new EventMetadata
                        {
                            ContainerName = "hannes",
                            CreatedDateTime = new DateTime(2016, 1, 1),
                            CreatorEmail = "hannes@sachsenhofer.com",
                            Event = "Hannes",
                            Password = "hannes"
                        }
                    }
                };
            }

            return JsonConvert.DeserializeObject<Metadata>(await file.DownloadTextAsync());
        }

        private static Metadata _metadata;
        public async Task<Metadata> GetOrLoad()
        {
            return _metadata = _metadata ?? await Load();
        }

        public async Task Save(Metadata metadata)
        {
            _metadata = metadata;

            var container = await GetMetadataContainer();
            var file = container.GetBlockBlobReference("metadata.json");

            await file.UploadTextAsync(JsonConvert.SerializeObject(metadata));
        }

        private CloudBlobContainer _container;

        private async Task<CloudBlobContainer> GetMetadataContainer()
        {
            if (_container == null)
            {
                var storageAccount = new CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(_settings.AzureStorageContainer, _settings.AzureStorageKey), true);
                var storageClient = storageAccount.CreateCloudBlobClient();
                var containerName = SasService.GetContainerName("_metadata");
                _container = storageClient.GetContainerReference(containerName);
                await _container.CreateIfNotExistsAsync();
            }
            return _container;
        }
    }
}
