using System;
using System.Net.Http;
using ModernHttpClient;
using Xamarin.Forms;

namespace Gruppenfoto.App
{
    public partial class EventSettingsPage
    {
        public EventSettingsPage(SettingsPage parentPage)
        {
            InitializeComponent();

            CancelButton.Clicked += async (sender, args) =>
            {
                await Navigation.PopModalAsync(true);
            };

            SaveButton.Clicked += async (sender, args) =>
            {
                if (Settings.Event != Event.Text || Settings.BackendUrl != Server.Text)
                {
                    // check if we can access a server with these new settings
                    try
                    {
                        using (var httpClient = new HttpClient(new NativeMessageHandler()))
                        {
                            var response = await httpClient.GetAsync($"{Server.Text.Trim('/')}/json/event/{Event.Text}");
                            response.EnsureSuccessStatusCode();
                        }
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("", "Dieses Event kann nicht gefunden werden.", "Alles klar");
                        return;
                    }

                    Settings.Event = Event.Text;
                    Settings.BackendUrl = Server.Text;

                    // clear the upload queue when switching to another event or server
                    Settings.UploadQueue = new string[0];

                    // reload the pictures list
                    ((App)Application.Current).PicturesPage.Refresh();

                    parentPage.Refresh();
                }
                await Navigation.PopModalAsync(true);
            };
        }

        protected override void OnAppearing()
        {
            Event.Text = Settings.Event;
            Server.Text = Settings.BackendUrl;
            base.OnAppearing();
        }
    }
}
