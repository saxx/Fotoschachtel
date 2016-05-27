using Fotoschachtel.Common;
using Foundation;
using HockeyApp;
using Refractored.XamForms.PullToRefresh.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

namespace Fotoschachtel.Ios
{
    [Register("AppDelegate")]
    public class AppDelegate : FormsApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            var manager = BITHockeyManager.SharedHockeyManager;
            manager.Configure("f591bd57701948d780b60965fea8c701");
            manager.DisableMetricsManager = false;
            manager.StartManager();

            PullToRefreshLayoutRenderer.Init();

            var uploader = new UploaderTask();
            MessagingCenter.Subscribe<StartUploadMessage>(this, "StartUpload", async message =>
            {
                await uploader.Start();
            });

            Forms.Init();
            LoadApplication(new Fotoschachtel.Common.App());
            return base.FinishedLaunching(app, options);
        }
    }
}