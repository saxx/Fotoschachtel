using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using PCLStorage;

namespace Gruppenfoto.App
{
    public static class Uploader
    {
        private static bool _isUploading;
        
        public static async Task UploadNextPicture()
        {
            if (_isUploading || !Settings.UploadQueue.Any())
            {
                return;
            }

            _isUploading = true;

            string nextFileName = null;
            IFile imageFile = null;
            try
            {
                nextFileName = Settings.UploadQueue.Last();
                imageFile = await FileSystem.Current.LocalStorage.GetFileAsync(nextFileName);

                byte[] imageBytes;
                using (var stream = await imageFile.OpenAsync(FileAccess.Read))
                {
                    imageBytes = new byte[stream.Length];
                    await stream.ReadAsync(imageBytes, 0, imageBytes.Length);
                }

                if (imageBytes.Length > 0)
                {
                    using (var client = new HttpClient())
                    {
                        var content = new MultipartFormDataContent();
                        var streamContent = new ByteArrayContent(imageBytes);
                        streamContent.Headers.ContentDisposition = ContentDispositionHeaderValue.Parse("form-data");
                        streamContent.Headers.ContentDisposition.Parameters.Add(new NameValueHeaderValue("name", "File"));
                        streamContent.Headers.ContentDisposition.Parameters.Add(new NameValueHeaderValue("filename", "\"" + nextFileName + "\""));
                        content.Add(streamContent);

                        var response = await client.PostAsync($"{Settings.BackendUrl.Trim('/')}/event/{Settings.EventId}/picture/", content);
                        response.EnsureSuccessStatusCode();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            finally
            {
                if (imageFile != null)
                {
                    await imageFile.DeleteAsync();
                }
                if (nextFileName != null)
                {
                    var remainingFiles = Settings.UploadQueue.ToList();
                    remainingFiles.Remove(nextFileName);
                    Settings.UploadQueue = remainingFiles.ToArray();
                }
                _isUploading = false;
            }
        }
    }
}
