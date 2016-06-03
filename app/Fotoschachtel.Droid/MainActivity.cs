using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Fotoschachtel.Common;
using HockeyApp;
using HockeyApp.Metrics;
using Refractored.XamForms.PullToRefresh.Droid;
using Xamarin.Forms;

namespace Fotoschachtel.Droid
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

            CrashManager.Register(this, "fdf7a0b02e0249b78ebe117e5003b5f2");
            MetricsManager.Register(this, Application, "fdf7a0b02e0249b78ebe117e5003b5f2");

            Forms.Init(this, bundle);
            ZXing.Net.Mobile.Forms.Android.Platform.Init();
            PullToRefreshLayoutRenderer.Init();
            LoadApplication(new App());
        }
    }
}

