﻿using Fotoschachtel.Common;
using Foundation;
using Gruppenfoto.App.iOS;
using Refractored.XamForms.PullToRefresh.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
namespace Gruppenfoto.App.iOS
{
    [Register("AppDelegate")]
    public class AppDelegate : FormsApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
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