using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Support.V7.App;

namespace Fotoschachtel.Droid
{
    [Activity(Label = "Fotoschachtel", Theme = "@style/MyTheme.Splash", MainLauncher = true, NoHistory = true)]
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