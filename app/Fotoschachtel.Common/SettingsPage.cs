using System;
using System.Net.Http;
using System.Threading.Tasks;
using Gruppenfoto.App;
using ModernHttpClient;
using Xamarin.Forms;

namespace Fotoschachtel.Common
{
    public class SettingsPage : ContentPage
    {
        private readonly HomePage _parentPage;
        private Label _eventLabel;
        private Label _serverLabel;

        public SettingsPage(HomePage parentPage)
        {
            _parentPage = parentPage;
            Title = "Einstellungen";

            BackgroundColor = Colors.BackgroundColor;
            Device.OnPlatform(
                iOS: () =>
                {
                    Padding = new Thickness(10, 20, 10, 10);
                },
                Android: () =>
                {
                    Padding = new Thickness(10, 10, 10, 10);
                });

            var backButton = Controls.Image("Fotoschachtel.Common.Images.back.png", 40, 40, async image =>
            {
                await Navigation.PopModalAsync(true);
            });
            var saveButton = Controls.Image("Fotoschachtel.Common.Images.save.png", 40, 40, async image =>
            {
                if (await Save())
                {
                    await Navigation.PopModalAsync(true);
                    await _parentPage.Refresh();
                }
            });

            var layout = new AbsoluteLayout();
            layout.Children.Add(new ScrollView
            {
                Padding = new Thickness(0, 40, 0, 0),
                Content = BuildContent()
            }, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.SizeProportional);
            layout.Children.Add(backButton, new Rectangle(0, 0, 40, 40));
            layout.Children.Add(saveButton, new Rectangle(1, 0, 40, 40), AbsoluteLayoutFlags.XProportional);

            Content = layout;
        }


        private StackLayout BuildContent()
        {
            var websiteButton = new Button
            {
                TextColor = Colors.BackgroundColor,
                BackgroundColor = Colors.FontColor,
                Text = "Website von Fotoschachtel öffnen"
            };
            websiteButton.Clicked += (sender, args) =>
            {
                Device.OpenUri(new Uri("https://gruppenfoto.sachsenhofer.com"));
            };

            _eventLabel = Controls.LabelMonospace(Settings.Event, async label =>
            {
                var result = await Navigation.Prompt("Event", "Bitte gib den Namen des Events an, mit dem du Fotoschachtel verwenden möchtest:");
                if (result != null)
                {
                    label.Text = result;
                }
            });

            _serverLabel = Controls.LabelMonospace(Settings.BackendUrl, async label =>
            {
                var result = await Navigation.Prompt("Server", "Bitte gib die Adresse des Servers an, mit dem du Fotoschachtel verwenden möchtest:");
                if (result != null)
                {
                    label.Text = result;
                }
            });

            return new StackLayout
            {
                VerticalOptions = LayoutOptions.StartAndExpand,
                Spacing = 60,
                Children =
                {
                    new StackLayout
                    {
                        VerticalOptions = LayoutOptions.Start,
                        Children =
                        {
                            new StackLayout
                            {
                                Spacing = 0,
                                Children  =
                                {
                                    Controls.Label("Event:"),
                                    _eventLabel,
                                    Controls.Separator(),
                                    Controls.Label("Server:"),
                                    _serverLabel,
                                }
                            }
                        }
                    },
                    new StackLayout
                    {
                        VerticalOptions = LayoutOptions.StartAndExpand
                    },
                    new StackLayout
                    {
                        VerticalOptions = LayoutOptions.End,
                        Spacing = 10,
                        Children =
                        {
                            new Label
                            {
                                FontAttributes = FontAttributes.Bold,
                                HorizontalTextAlignment = TextAlignment.Center,
                                TextColor = Colors.FontColor,
                                Text = "Fotoschachtel ist ein Hobbyprojekt von\nHannes 'saxx' Sachsenhofer.",
                            },
                            new Label
                            {
                                TextColor = Colors.FontColor,
                                Text =
                                    "Du darfst Gruppenfoto gerne benutzen, so viel du magst; "
                                    + "aber bitte denk daran: Gruppenfoto wird ohne jede ausdrückliche oder implizierte Garantie bereitgestellt, einschließlich der Garantie zur Benutzung "
                                    + "für den vorgesehenen oder einem bestimmten Zweck sowie jeglicher Rechtsverletzung, jedoch nicht darauf beschränkt. "
                                    + "In keinem Fall ist der Autor oder Copyright-Inhaber für jeglichen Schaden oder sonstige Ansprüche haftbar zu machen, "
                                    + "ob infolge der Erfüllung eines Vertrages, eines Deliktes oder anders im Zusammenhang mit der Software oder sonstiger Verwendung der Software entstanden."
                            },
                            websiteButton
                        }
                    }
                }
            };
        }

        private async Task<bool> Save()
        {
            if (Settings.BackendUrl != _serverLabel.Text || Settings.Event != _eventLabel.Text)
            {
                // check if we can access a server with these new settings
                try
                {
                    using (var httpClient = new HttpClient(new NativeMessageHandler()))
                    {
                        var response = await httpClient.GetAsync($"{_serverLabel.Text.Trim('/')}/json/event/{_eventLabel.Text}");
                        response.EnsureSuccessStatusCode();
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("", "Es gibt ein Problem mit den Event- oder Servereinstellungen: " + ex.Message, "Alles klar");
                    return false;
                }

                Settings.Event = _eventLabel.Text;
                Settings.BackendUrl = _serverLabel.Text;

                // clear the upload queue when switching to another event or server
                Settings.UploadQueue = new string[0];
            }

            return true;
        }
    }
}
