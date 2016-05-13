using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Fotoschachtel.Common;
using Refractored.XamForms.PullToRefresh.Droid;
using Xamarin.Forms;

namespace Gruppenfoto.App.Droid
{
    [Activity(Label = "Fotoschachtel", Icon = "@drawable/icon", ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            MessagingCenter.Subscribe<StartUploadMessage>(this, "StartUpload", message => {
                StartService(new Intent(this, typeof(UploaderService)));
            });

            base.OnCreate(bundle);

            Forms.Init(this, bundle);
            PullToRefreshLayoutRenderer.Init();
            LoadApplication(new Fotoschachtel.Common.App());
        }
    }
}

