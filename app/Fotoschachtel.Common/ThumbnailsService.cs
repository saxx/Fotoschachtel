using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace Fotoschachtel.Common
{
    public class ThumbnailsService
    {
        public static async Task UpdateThumbnails()
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.PostAsync(Settings.ThumbnailsUri, null);
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
