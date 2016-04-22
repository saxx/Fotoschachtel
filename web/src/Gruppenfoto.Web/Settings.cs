using Microsoft.Extensions.Configuration;

namespace Gruppenfoto.Web
{
    public class Settings
    {
        public Settings(IConfiguration configuration)
        {
            AzureStorageContainer = configuration.Get("AzureStorageContainer", "");
            AzureStorageKey = configuration.Get("AzureStorageKey", "");
        }


        public string AzureStorageContainer { get; set; }
        public string AzureStorageKey { get; set; }
    }
}
