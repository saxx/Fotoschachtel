using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using ModernHttpClient;

namespace Fotoschachtel.Common
{
    public class ThumbnailsService
    {
        public static async Task UpdateThumbnails()
        {
            try
            {
                using (var httpClient = new HttpClient(new NativeMessageHandler()))
                {
                    var response = await httpClient.PostAsync($"{Settings.BackendUrl}/json/event/{Settings.Event}/thumbnails", null);
                    response.EnsureSuccessStatusCode();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}
