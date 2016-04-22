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

            HomepageButton.Clicked += (sender, args) =>
            {
                Device.OpenUri(new Uri("https://gruppenfoto.sachsenhofer.com"));
            };

            Disappearing += (sender, args) =>
            {
                if (Settings.Event != EventCell.Text || Settings.BackendUrl != ServerCell.Text)
                {
                    Settings.Event = EventCell.Text;
                    Settings.BackendUrl = ServerCell.Text;

                    // clear the upload queue when switching to another event or server
                    Settings.UploadQueue = new string[0];

                    // reload the pictures list
                    ((App) Application.Current).PicturesPage.Refresh();
                }
            };
        }

        protected override void OnAppearing()
        {
            EventCell.Text = Settings.Event;
            ServerCell.Text = Settings.BackendUrl;

            base.OnAppearing();
        }
    }
}
