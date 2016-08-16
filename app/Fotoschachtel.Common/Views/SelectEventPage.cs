using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;

namespace Fotoschachtel.Common.Views
{
    public class SelectEventPage : ContentPage
    {
        private readonly Action _closeCallback;

        public SelectEventPage(Action closeCallback)
        {
            _closeCallback = closeCallback;

            BackgroundColor = Colors.BackgroundColor;
            Padding = new Thickness(10);

            var isFirstAppearance = true;
            Appearing += async (sender, args) =>
            {
                if (!isFirstAppearance)
                {
                    return;
                }
                isFirstAppearance = false;

                var header = Settings.LastRunDateTime.HasValue ? "Code bei der Hand?" : "Willkommen bei Fotoschachtel!";
                var alertResult = await DisplayAlert(header, "Du kannst direkt einen Fotoschachtel-Code einscannen, um Fotoschachtel gleich mit dem dazugehörigen Event zu verknüpfen.\n\n" +
                                                     "Oder du kannst manuell ein Event und dessen Passwort eintippen. Dann kannst du auch ein neues Event erstellen, wenn du möchtest.", "Manuell eintippen", "Code scannen");

                if (!alertResult)
                {
                    await UseCodeScanner();
                }
                else
                {
                    UseForm();
                }
            };
        }

        private async Task UseCodeScanner()
        {
            var scanPage = new ZXingScannerPage
            {
                DefaultOverlayBottomText = "",
                DefaultOverlayTopText = "",
            };

            scanPage.OnScanResult += (result) =>
            {
                scanPage.IsScanning = false;
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Navigation.PopModalAsync();

                    if (!string.IsNullOrWhiteSpace(result.Text) && result.Text.Length > 5 && result.Text.Contains(":"))
                    {
                        var a = result.Text.Split(':');
                        if (await Save(a[0], a[1]))
                        {
                            _closeCallback.Invoke();
                            await Navigation.PopModalAsync();
                        }
                    }
                    else
                    {
                        await DisplayAlert("Oje", "Das scheint kein gültiger Fotoschachtel-Code zu sein.", "Achso");
                    }
                });
            };

            await Navigation.PushModalAsync(scanPage);
        }


        private void UseForm()
        {
            var layout = new StackLayout
            {
                VerticalOptions = LayoutOptions.StartAndExpand
            };

            #region Logo

            var logo = Controls.Image("logo", 100);
            logo.HorizontalOptions = LayoutOptions.Center;
            layout.Children.Add(logo);

            #endregion

            #region Input fields

            var eventEntry = Controls.Entry(Settings.Event);
            var passwordEntry = Controls.Password("");

            layout.Children.Add(new StackLayout
            {
                Spacing = 0,
                Children =
                {
                    Controls.Label("Name des Events:"),
                    eventEntry
                }
            });

            layout.Children.Add(new StackLayout
            {
                Spacing = 0,
                Children =
                {
                    Controls.Label("Passwort für das Event:"),
                    passwordEntry
                }
            });
            #endregion

            #region Buttons

            var okButton = Controls.Image("save.png", 40, async image =>
            {
                if (await Save(eventEntry.Text, passwordEntry.Text))
                {
                    _closeCallback.Invoke();
                    await Navigation.PopModalAsync();
                }
            });
            okButton.HorizontalOptions = LayoutOptions.End;
            var cancelButton = Controls.Image("cancel.png", 40, async image =>
            {
                await Navigation.PopModalAsync();
            });
            cancelButton.HorizontalOptions = LayoutOptions.StartAndExpand;

            layout.Children.Add(new StackLayout
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Orientation = StackOrientation.Horizontal,
                Children = { cancelButton, okButton },
            });

            #endregion

            #region New event button

            layout.Children.Add(Controls.Separator());
            var newEventButton = Controls.Button("Neues Event anlegen", button =>
            {
                Device.OpenUri(new Uri("https://fotoschachtel.sachsenhofer.com/create-event"));
            });
            layout.Children.Add(newEventButton);

            #endregion

            Content = layout;
        }


        private async Task<bool> Save(string @event, string password)
        {
            if (string.IsNullOrWhiteSpace(@event))
            {
                return false;
            }

            // check if we can access a server with these new settings
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(Settings.GetSasTokenUri(@event, password));

                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        await DisplayAlert("", "Hmm, das angegebene Event scheint nicht zu existieren.", "Alles klar");
                        return false;
                    }
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        await DisplayAlert("", "Hmm, das angegebene Passwort scheint nicht zu stimmen.", "Alles klar");
                        return false;
                    }
                    response.EnsureSuccessStatusCode();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("", "Bitte überprüfe deine Angaben.\n" + ex.Message, "Alles klar");
                return false;
            }

            Settings.Event = @event;
            Settings.EventPassword = password;

            // clear the upload queue when switching to another event or server
            Settings.UploadQueue = new string[0];
            return true;
        }
    }
}
