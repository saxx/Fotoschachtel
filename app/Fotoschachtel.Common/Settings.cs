using System;
using System.Net.Http;
using System.Threading.Tasks;
using ModernHttpClient;
using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace Fotoschachtel.Common
{
    public static class Settings
    {
        private static ISettings AppSettings => CrossSettings.Current;


        public static string BackendUrl
        {
            get { return AppSettings.GetValueOrDefault("BackendUrl", "https://fotoschachtel.sachsenhofer.com"); }
            set { AppSettings.AddOrUpdateValue("BackendUrl", value.Trim('/')); }
        }


        public static string Event
        {
            get { return AppSettings.GetValueOrDefault("Event", "sandbox"); }
            set { AppSettings.AddOrUpdateValue("Event", value); }
        }


        public static string[] UploadQueue
        {
            get
            {
                var json = AppSettings.GetValueOrDefault("UploadQueue", "{}");
                try
                {
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(json);
                }
                catch
                {
                    return new string[0];
                }
            }
            set
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(value);
                AppSettings.AddOrUpdateValue("UploadQueue", json);
            }
        }


        public static async Task<SasToken> GetSasToken()
        {
            var json = AppSettings.GetValueOrDefault("SasToken", "{}");
            try
            {
                var sasToken = Newtonsoft.Json.JsonConvert.DeserializeObject<SasToken>(json);
                if (sasToken.SasExpiration <= DateTime.UtcNow || sasToken.EventId != Event)
                {
                    return await GetNewSasToken();
                }
                return sasToken;
            }
            catch
            {
                return await GetNewSasToken();
            }
        }


        private static async Task<SasToken> GetNewSasToken()
        {
            using (var httpClient = new HttpClient(new NativeMessageHandler()))
            {
                string response;
                try
                {
                    response = await httpClient.GetStringAsync($"{BackendUrl}/json/event/{Event}");
                }
                catch (Exception ex)
                {
                    throw new Exception("Unable to load a new SAS token from the backend: " + ex.Message);
                }

                AppSettings.AddOrUpdateValue("SasToken", response);
                try
                {
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<SasToken>(response);
                }
                catch (Exception ex)
                {
                    throw new Exception("Unable to de-serialize SAS token from the backend: " + ex.Message);
                }
            }
        }


        public class SasToken
        {
            public string ContainerUrl { get; set; }
            public string SasQueryString { get; set; }
            public string EventId { get; set; }
            public DateTime SasExpiration { get; set; }
            public string SasListUrl { get; set; }
        }
    }
}
