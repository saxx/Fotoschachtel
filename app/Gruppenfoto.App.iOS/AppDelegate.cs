using Foundation;
using Gruppenfoto.App.iOS;
using Refractored.XamForms.PullToRefresh.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using XLabs.Forms.Controls;

[assembly: ExportRenderer(typeof(TabbedPage), typeof(TabbedPageRenderer))]

namespace Gruppenfoto.App.iOS
{
    [Register("AppDelegate")]
    public class AppDelegate : FormsApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            DependencyService.Register<ImageButtonRenderer>();
            PullToRefreshLayoutRenderer.Init();

            var uploader = new UploaderTask();
            MessagingCenter.Subscribe<StartUploadMessage>(this, "StartUpload", async message =>
            {
                await uploader.Start();
            });

            Forms.Init();
            LoadApplication(new App());
            return base.FinishedLaunching(app, options);
        }
    }

    public class TabbedPageRenderer : TabbedRenderer
    {
        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);

            //TabBar.TintColor = UIColor.White;
            //TabBar.SelectedImageTintColor = UIColor.White;
            //TabBar.BarTintColor = ;
            //TabBar.TintAdjustmentMode = UIViewTintAdjustmentMode.Normal;
            TabBar.BarTintColor = UIColor.FromRGB(38, 169, 224);
            TabBar.SelectedImageTintColor = UIColor.FromRGB(240, 241, 241);
            TabBar.TintColor = UIColor.Green;
            //TabBar.BackgroundColor = UIColor.Yellow;
        }
    }
}