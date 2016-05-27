using System;
using Xamarin.Forms;

namespace Fotoschachtel.Common.Views
{
    public class SettingsPage : ContentPage
    {
        private readonly Action _closeCallback;

        public SettingsPage(Action closeCallback)
        {
            _closeCallback = closeCallback;

            BackgroundColor = Colors.BackgroundColor;
            Padding = new Thickness(10);

            var backButton = Controls.Image("Fotoschachtel.Common.Images.back.png", 40, 40, async image =>
            {
                await Navigation.PopModalAsync(true);
            });

            var layout = new AbsoluteLayout();
            layout.Children.Add(new ScrollView
            {
                Padding = new Thickness(0, 50, 0, 0),
                Content = BuildContent()
            }, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.SizeProportional);
            layout.Children.Add(backButton, new Rectangle(0, 0, 40, 40));

            Content = layout;
        }


        private StackLayout BuildContent()
        {
            #region Logo
            var logoImage = Controls.Image("logo", 200);
            logoImage.HorizontalOptions = LayoutOptions.Center;
            #endregion

            #region Event

            var eventText = new FormattedString();
            eventText.Spans.Add(new Span { Text = "Fotoschachtel ist derzeit mit dem Event " });
            eventText.Spans.Add(new Span { Text = Settings.Event, FontAttributes = FontAttributes.Bold });
            eventText.Spans.Add(new Span { Text = " verknüpft." });
            var eventLabel = Controls.Label(eventText);
            var eventButton = Controls.Button("Event ändern", async button =>
            {
                var selectEventPage = new SelectEventPage(async () =>
                {
                    _closeCallback.Invoke();
                    await Navigation.PopModalAsync();
                });
                await Navigation.PushModalAsync(selectEventPage);
            });

            #endregion

            #region Copyright

            var authorLabel = Controls.Label("Fotoschachtel ist ein Hobbyprojekt von Hannes 'saxx' Sachsenhofer");
            authorLabel.HorizontalTextAlignment = TextAlignment.Center;
            authorLabel.FontAttributes = FontAttributes.Bold;
            var copyrightLabel = Controls.Label("Du darfst Fotoschachtel gerne benutzen, so viel du magst; "
                       + "aber bitte denk daran: Fotoschachtel wird ohne jede ausdrückliche oder implizierte Garantie bereitgestellt, einschließlich der Garantie zur Benutzung "
                       + "für den vorgesehenen oder einem bestimmten Zweck sowie jeglicher Rechtsverletzung, jedoch nicht darauf beschränkt. "
                       + "In keinem Fall ist der Autor oder Copyright-Inhaber für jeglichen Schaden oder sonstige Ansprüche haftbar zu machen, "
                       + "ob infolge der Erfüllung eines Vertrages, eines Deliktes oder anders im Zusammenhang mit der Software oder sonstiger Verwendung der Software entstanden.");
            var websiteButton = Controls.Button("Website von Fotoschachtel öffnen", button =>
            {
                Device.OpenUri(new Uri("https://fotoschachtel.sachsenhofer.com"));
            });

            #endregion

            return new StackLayout
            {
                VerticalOptions = LayoutOptions.StartAndExpand,
                Spacing = 40,
                Children =
                {
                    logoImage,
                    new StackLayout
                    {
                        VerticalOptions = LayoutOptions.Start,
                        Spacing = 5,
                        Children =
                        {
                            eventLabel,
                            eventButton
                        }
                    },
                    new StackLayout
                    {
                        VerticalOptions = LayoutOptions.End,
                        Spacing = 5,
                        Children =
                        {
                            authorLabel,
                            copyrightLabel,
                            websiteButton
                        }
                    }
                }
            };
        }
    }
}

