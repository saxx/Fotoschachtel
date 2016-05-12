using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Fotoschachtel.Common;
using Java.Lang;
using ModernHttpClient;
using PCLStorage;
using Xamarin.Forms;

namespace Gruppenfoto.App.Droid
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
            var nextFileName = Settings.UploadQueue.FirstOrDefault();
            if (nextFileName == null)
            {
                return;
            }
            Settings.UploadQueue = Settings.UploadQueue.Skip(1).ToArray();

            IFile nextFile;
            try
            {
                nextFile = await FileSystem.Current.LocalStorage.GetFileAsync(nextFileName);
            }
            catch (Java.Lang.Exception ex)
            {
                Console.WriteLine(ex);

                // do nothing here, the file does not exist apparently.
                // we want it removed from the queue
                return;
            }

            var sasToken = await Settings.GetSasToken();

            try
            {
                using (var stream = await nextFile.OpenAsync(FileAccess.Read))
                {
                    try
                    {
                        using (var httpClient = new HttpClient(new NativeMessageHandler()))
                        {
                            var content = new StreamContent(stream);
                            content.Headers.Add("x-ms-blob-type", "BlockBlob");
                            var response = await httpClient.PutAsync($"{sasToken.ContainerUrl}/{Guid.NewGuid()}{sasToken.SasQueryString}", content);
                            response.EnsureSuccessStatusCode();

                            await ThumbnailsService.UpdateThumbnails();
                        }
                    }
                    catch (Java.Lang.Exception ex)
                    {
                        Console.WriteLine(ex);

                        // do nothing here, we cannot upload the file
                        // but we want to retry
                        Settings.UploadQueue = Settings.UploadQueue.Concat(new[] {nextFileName}).ToArray();
                        return;
                    }
                }
                await nextFile.DeleteAsync();
            }
            catch (Java.Lang.Exception ex)
            {
                Console.WriteLine(ex);

                // do nothing here, we cannot open or delete the file
                // we want it removed from the queue
            }
        }
    }
}