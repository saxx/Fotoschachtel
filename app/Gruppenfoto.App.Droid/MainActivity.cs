using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Xamarin.Forms;

namespace Gruppenfoto.App.Droid
{
    [Activity(Label = "Gruppenfoto.App", Icon = "@drawable/icon", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            MessagingCenter.Subscribe<StartUploadMessage>(this, "StartUpload", message => {
                StartService(new Intent(this, typeof(UploaderService)));
            });
            MessagingCenter.Subscribe<PauseUploadMessage>(this, "PauseUpload", message => {
                StopService(new Intent(this, typeof(UploaderService)));
            });

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());
        }
    }
}

