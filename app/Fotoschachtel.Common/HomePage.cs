using System.Threading.Tasks;
using Xamarin.Forms;

namespace Fotoschachtel.Common
{
    public class HomePage : ContentPage
    {
        private readonly HomePageMiddleContent _middleContent;

        public HomePage()
        {
            BackgroundColor = Colors.BackgroundColor;
            Device.OnPlatform(
                iOS: () =>
                {
                    Padding = new Thickness(0, 20, 0, 0);
                },
                Android: () =>
                {
                    Padding = new Thickness(0, 0, 0, 0);
                });

            _middleContent = new HomePageMiddleContent(this);
            Content = new StackLayout
            {
                Spacing = 0,
                Children =
                {
                    new HomePageTopContent(this),
                    _middleContent,
                    new HomePageBottomContent(this)
                }
            };
        }

        public async Task Refresh()
        {
            await _middleContent.Refresh();
        }
    }
}
