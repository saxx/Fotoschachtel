using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Gruppenfoto.App;
using PCLStorage;
using Plugin.Media.Abstractions;
using Xamarin.Forms;

namespace Fotoschachtel.Common
{
    public class HomePageBottomContent : StackLayout
    {
        private readonly HomePage _parentPage;

        public HomePageBottomContent(HomePage parentPage)
        {
            _parentPage = parentPage;
            BackgroundColor = Colors.BackgroundColor;
            VerticalOptions = LayoutOptions.End;
            HorizontalOptions = LayoutOptions.FillAndExpand;
            Orientation = StackOrientation.Horizontal;

            Spacing = 5;
            Padding = new Thickness(5);

            var libraryButton = new Button
            {
                Text = "Auswählen",
                //HeightRequest = 40,
                HorizontalOptions = LayoutOptions.StartAndExpand,
                BackgroundColor = Colors.FontColor,
                TextColor = Colors.BackgroundColor,
            };

            var cameraButton = new Button
            {
                Text = "Knipsen",
                //HeightRequest = 40,
                HorizontalOptions = LayoutOptions.EndAndExpand,
                BackgroundColor = Colors.FontColor,
                TextColor = Colors.BackgroundColor
            };

            var media = Plugin.Media.CrossMedia.Current;

            libraryButton.IsEnabled = media.IsPickPhotoSupported;
            libraryButton.Clicked += async (sender, args) =>
            {
                await AddUpload(await media.PickPhotoAsync());
            };
            
            cameraButton.IsEnabled = media.IsTakePhotoSupported;
            cameraButton.Clicked += async (sender, args) =>
            {
                await AddUpload(await media.TakePhotoAsync(new StoreCameraMediaOptions
                {
                    SaveToAlbum = true,
                    DefaultCamera = CameraDevice.Rear
                }));
            };

            MessagingCenter.Subscribe<UploadFinishedMessage>(this, "UploadFinished", async message =>
            {
                Device.BeginInvokeOnMainThread(UpdateQueueLabel);
                await _parentPage.Refresh();
            });

            UpdateQueueLabel();

            Children.Add(libraryButton);
            Children.Add(cameraButton);
        }


        private async Task AddUpload(MediaFile file)
        {
            if (file == null)
            {
                return;
            }

            var fileName = Guid.NewGuid() + ".jpg";
            byte[] imageBytes;
            using (var memoryStream = new MemoryStream())
            {
                file.GetStream().CopyTo(memoryStream);
                imageBytes = memoryStream.ToArray();
                file.Dispose();
            }

            var imageFile = await FileSystem.Current.LocalStorage.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);
            using (var stream = await imageFile.OpenAsync(FileAccess.ReadAndWrite))
            {
                await stream.WriteAsync(imageBytes, 0, imageBytes.Length);
            }

            Settings.UploadQueue = Settings.UploadQueue.Concat(new[] { fileName }).ToArray();

            UpdateQueueLabel();

            MessagingCenter.Send(new StartUploadMessage(), "StartUpload");
        }


        public void UpdateQueueLabel()
        {
            // do nothing here for now, because we don't have a queue label
        }
    }
}
