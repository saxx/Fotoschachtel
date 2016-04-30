using Xamarin.Forms;

namespace Gruppenfoto.App
{
    public class App : Application
    {
        public App()
        {
            MainPage = new TabbedPage
            {
                Children =
                {
                    new StartPage()
                    , PicturesPage
                    , new SettingsPage()
                }
                , BackgroundColor = Colors.BackgroundColor
            };
        }

        public PicturesPage PicturesPage { get; } = new PicturesPage();

        protected override void OnStart()
        {
            MessagingCenter.Send(new StartUploadMessage(), "StartUpload");
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
            MessagingCenter.Send(new StartUploadMessage(), "StartUpload");
        }
    }
}
