using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Support.V7.App;
using Gruppenfoto.App.Droid;

namespace Fotoschachtel.Droid
{
    [Activity(Theme = "@style/MyTheme.Splash", MainLauncher = true, NoHistory = true)]
    public class SplashActivity : AppCompatActivity
    {

        protected override void OnResume()
        {
            base.OnResume();

            Task startupWork = new Task(() =>
            {
            });

            startupWork.ContinueWith(t =>
            {
                StartActivity(new Intent(Application.Context, typeof(MainActivity)));
            }, TaskScheduler.FromCurrentSynchronizationContext());

            startupWork.Start();
        }
    }
}