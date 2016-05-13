using Fotoschachtel.Common.Views;
using Xamarin.Forms;

namespace Fotoschachtel.Common
{
    public class App : Application
    {
        public App()
        {
            MainPage = new HomePage();
        }

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
