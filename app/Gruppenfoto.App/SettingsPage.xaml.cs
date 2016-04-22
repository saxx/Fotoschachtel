using System;
using Xamarin.Forms;

namespace Gruppenfoto.App
{
    public partial class SettingsPage
    {
        public SettingsPage()
        {
            InitializeComponent();

            CopyrightLabel.Text = "Gruppenfoto ist ein Hobbyprojekt von\nHannes 'saxx' Sachsenhofer.";
            
            var eventSettingsPage = new EventSettingsPage(this);
            EventSettingsButton.Clicked += async (sender, args) =>
            {
                await Navigation.PushModalAsync(eventSettingsPage, true);
            };

            HomepageButton.Clicked += (sender, args) =>
            {
                Device.OpenUri(new Uri("https://gruppenfoto.sachsenhofer.com"));
            };

        }

        public void Refresh()
        {
            var fs = new FormattedString();
            fs.Spans.Add(new Span { Text = "Du bist derzeit mit dem Event " });
            fs.Spans.Add(new Span { Text = Settings.Event, FontAttributes = FontAttributes.Bold });
            fs.Spans.Add(new Span { Text = " verknüpft" });
            EventSettingsLabel.FormattedText = fs;
        }

        protected override void OnAppearing()
        {
            Refresh();
            base.OnAppearing();
        }
    }
}
