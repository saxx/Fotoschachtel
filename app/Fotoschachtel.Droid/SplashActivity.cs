using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using HockeyApp;
using HockeyApp.Metrics;

namespace Fotoschachtel.Droid
{
    [Activity(Label = "Fotoschachtel", Theme = "@style/MyTheme.Splash", MainLauncher = true, NoHistory = true)]
    public class SplashActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            CrashManager.Register(this, "fdf7a0b02e0249b78ebe117e5003b5f2");
            MetricsManager.Register(this, Application, "fdf7a0b02e0249b78ebe117e5003b5f2");
        }


        protected override void OnResume()
        {
            base.OnResume();
            CrashManager.Register(this, "fdf7a0b02e0249b78ebe117e5003b5f2");
            MetricsManager.Register(this, Application, "fdf7a0b02e0249b78ebe117e5003b5f2");

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