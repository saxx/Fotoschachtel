using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Java.Lang;
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
            System.Threading.Tasks.Task.Run(() =>
            {
                while (Settings.UploadQueue.Any())
                {
                    Uploader.UploadNextPicture().ContinueWith(x =>
                    {
                        MessagingCenter.Send(new UploadFinishedMessage(), "UploadFinished");
                    });
                    Thread.Sleep(5000);
                }
                StopSelf();
            });

            return StartCommandResult.Sticky;
        }
    }
}