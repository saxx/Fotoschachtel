using Xamarin.Forms;

namespace Fotoschachtel.Common
{
    public static class Fonts
    {
        public static string Normal => Device.OnPlatform("GillSans-Light", "normal", null);
        public static string Monospace => Device.OnPlatform("CourierNewPSMT", "monospace", null);
    }
}
