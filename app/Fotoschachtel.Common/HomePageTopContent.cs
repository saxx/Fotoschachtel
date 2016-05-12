using Xamarin.Forms;

namespace Fotoschachtel.Common
{
    public class HomePageTopContent : StackLayout
    {
        private readonly HomePage _parentPage;
        private static SettingsPage _settingsPage;

        public HomePageTopContent(HomePage parentPage)
        {
            _parentPage = parentPage;
            BackgroundColor = Colors.BackgroundColor;
            VerticalOptions = LayoutOptions.Start;
            Orientation = StackOrientation.Horizontal;

            Children.Add(new Image
            {
                Source = ImageSource.FromResource("Fotoschachtel.Common.Images.fotoschachtel.png", GetType()),
                HeightRequest = 40,
                WidthRequest = 40
            });

            Children.Add(new Label
            {
                HorizontalTextAlignment = TextAlignment.Center,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Text = "Fotoschachtel",
                HeightRequest = 30,
                VerticalTextAlignment = TextAlignment.Center,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.FontColor
            });

            var settingsButton = new Image
            {
                Source = ImageSource.FromResource("Fotoschachtel.Common.Images.settings.png", GetType()),
                HeightRequest = 40,
                WidthRequest = 40
            };
            settingsButton.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(async () =>
                {
                    await Navigation.PushModalAsync(_settingsPage = _settingsPage ?? new SettingsPage(_parentPage), true);
                })
            });

            Children.Add(settingsButton);
        }
    }
}
