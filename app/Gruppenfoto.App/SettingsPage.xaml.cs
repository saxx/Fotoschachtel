using System.Linq;
using PCLStorage;
using Xamarin.Forms;

namespace Gruppenfoto.App
{
    public partial class SettingsPage
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            EventCell.Text = Settings.EventId;
            ServerCell.Text = Settings.BackendUrl;

            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            if (Settings.EventId != EventCell.Text || Settings.BackendUrl != ServerCell.Text)
            {
                Settings.EventId = EventCell.Text;
                Settings.BackendUrl = ServerCell.Text;

                // clear the upload queue when switching to another event or server
                Settings.UploadQueue = new string[0];

                // reload the pictures list
                ((App)Application.Current).PicturesPage.Refresh();

                // delete all stored thumbnails
                var imageFiles = FileSystem.Current.LocalStorage.GetFilesAsync().Result;
                foreach (var file in imageFiles.Where(x => x.Name.StartsWith("thumbnail_")))
                {
                    file.DeleteAsync().GetAwaiter().GetResult();
                }
            }
            base.OnDisappearing();
        }
    }
}
