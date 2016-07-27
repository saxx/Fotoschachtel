using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Fotoschachtel.Common;
using Java.Lang;
using ModernHttpClient;
using Xamarin.Forms;

namespace Fotoschachtel.Droid
{
    [Service]
    public class UploaderService : Service
    {
        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            Task.Run(() =>
            {
                while (Settings.UploadQueue.Any())
                {
                    UploadNextPicture().ContinueWith(x =>
                    {
                        MessagingCenter.Send(new UploadFinishedMessage(), "UploadFinished");
                    });
                    Thread.Sleep(5000);
                }
                StopSelf();
            });

            return StartCommandResult.Sticky;
        }


        private async Task UploadNextPicture()
        {
            var nextFilePath = Settings.UploadQueue.FirstOrDefault();
            if (nextFilePath == null)
            {
                return;
            }
            Settings.UploadQueue = Settings.UploadQueue.Skip(1).ToArray();

            var sasToken = await Settings.GetSasToken();
            try
            {
                using (var httpClient = new HttpClient(new NativeMessageHandler()))
                {
                    var nextFile = File.Open(nextFilePath, FileMode.Open, FileAccess.Read);
                    var content = new StreamContent(nextFile);
                    content.Headers.Add("x-ms-blob-type", "BlockBlob");
                    var response = await httpClient.PutAsync($"{sasToken.ContainerUrl}/{Guid.NewGuid()}{sasToken.SasQueryString}", content);
                    response.EnsureSuccessStatusCode();

                    await ThumbnailsService.UpdateThumbnails();
                }
            }
            catch
            {
                // do nothing here, we cannot upload the file
            }

            // DependencyService.Get<ITemporaryPictureStorage>().Delete(nextFileName);
        }
    }
}