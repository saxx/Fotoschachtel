using Xamarin.Forms;

namespace Gruppenfoto.App
{
    public class App : Application
    {
        public App()
        {
            MainPage = new TabbedPage()
            {
                Title = "Gruppenfoto"
                ,
                Children =
                {
                    new StartPage
                    {
                        Title = "Foto knipsen"
                    }
                    , PicturesPage
                    , new SettingsPage
                    {
                        Title = "Einstellungen"
                    }
                }
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
