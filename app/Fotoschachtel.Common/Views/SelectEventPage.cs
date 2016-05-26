using System;
using System.Net.Http;
using System.Threading.Tasks;
using ModernHttpClient;
using Xamarin.Forms;

namespace Fotoschachtel.Common.Views
{
    public class SelectEventPage : ContentPage
    {
        private readonly HomePage _parentPage;

        public SelectEventPage(HomePage parentPage)
        {
            _parentPage = parentPage;

            BackgroundColor = Colors.BackgroundColor;
            Padding = new Thickness(10);

            var layout = new AbsoluteLayout();
            /*layout.Children.Add(new ScrollView
            {
                Padding = new Thickness(0, 50, 0, 0),
                Content = BuildContent()
            }, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.SizeProportional);
            layout.Children.Add(backButton, new Rectangle(0, 0, 40, 40));
            layout.Children.Add(_saveButton, new Rectangle(1, 0, 40, 40), AbsoluteLayoutFlags.XProportional);*/

            Content = layout;
        }

    }
}
