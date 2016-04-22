using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PCLStorage;
using Plugin.Media.Abstractions;
using Xamarin.Forms;

namespace Gruppenfoto.App
{
    public partial class StartPage
    {
        public StartPage()
        {
            InitializeComponent();
            var media = Plugin.Media.CrossMedia.Current;

            CameraButton.IsEnabled = media.IsTakePhotoSupported;
            CameraButton.Clicked += async (sender, args) =>
            {
                await AddUpload(await media.TakePhotoAsync(new StoreCameraMediaOptions
                {
                    SaveToAlbum = true,
                    DefaultCamera = CameraDevice.Rear
                }));
            };

            LibraryButton.IsEnabled = media.IsPickPhotoSupported;
            LibraryButton.Clicked += async (sender, args) =>
            {
                await AddUpload(await media.PickPhotoAsync());
            };

            MessagingCenter.Subscribe<UploadFinishedMessage>(this, "UploadFinished", message =>
            {
                Device.BeginInvokeOnMainThread(UpdateQueueLabel);
            });

            UpdateQueueLabel();
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
            ActivityIndicator.IsVisible = ActivityIndicator.IsRunning = Settings.UploadQueue.Length > 0;
        }
    }
}
