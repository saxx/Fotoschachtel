using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ModernHttpClient;
using Xamarin.Forms;

namespace Fotoschachtel.Common.Views
{
    public class SelectEventPage : ContentPage
    {
        private readonly Action _closeCallback;
        private readonly Entry _eventEntry;
        private readonly Entry _passwordEntry;

        public SelectEventPage(Action closeCallback)
        {
            _closeCallback = closeCallback;

            BackgroundColor = Colors.BackgroundColor;
            Padding = new Thickness(10);

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

            _eventEntry = Controls.Entry(Settings.Event);
            _passwordEntry = Controls.Password("");

            layout.Children.Add(new StackLayout
            {
                Spacing = 0,
                Children =
                {
                    Controls.Label("Name des Events:"),
                    _eventEntry
                }
            });

            layout.Children.Add(new StackLayout
            {
                Spacing = 0,
                Children =
                {
                    Controls.Label("Passwort für das Event:"),
                    _passwordEntry
                }
            });
            #endregion

            #region Buttons

            var okButton = Controls.Image("save.png", 40, async image =>
            {
                if (await Save())
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

        private async Task<bool> Save()
        {
            if (string.IsNullOrWhiteSpace(_eventEntry.Text))
            {
                return false;
            }

            // check if we can access a server with these new settings
            try
            {
                using (var httpClient = new HttpClient(new NativeMessageHandler()))
                {
                    var response = await httpClient.GetAsync(Settings.GetSasTokenUri(_eventEntry.Text, _passwordEntry.Text));

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

            Settings.Event = _eventEntry.Text;
            Settings.EventPassword = _passwordEntry.Text;

            // clear the upload queue when switching to another event or server
            Settings.UploadQueue = new string[0];
            return true;
        }
    }
}
