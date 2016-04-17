using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PCLStorage;
using Plugin.Media.Abstractions;
using Xamarin.Forms;
using XLabs.Enums;
using XLabs.Forms.Controls;

namespace Gruppenfoto.App
{
    public class StartPage : ContentPage
    {
        private readonly Label _queueLabel;

        public StartPage()
        {
            #region Camera button
            var cameraButton = new ImageButton
            {
                Text = "Foto knipsen",
                Source = "camera.png",
                HeightRequest = 80,
                ImageHeightRequest = 50,
                ImageWidthRequest = 50,
                Orientation = ImageOrientation.ImageOnTop
            };
            cameraButton.Clicked += async (sender, args) =>
            {
                var media = Plugin.Media.CrossMedia.Current;
                if (!media.IsPickPhotoSupported)
                {
                    await DisplayAlert("Oje", "Es sieht so aus, wie wenn du auf deinem Gerät keine Fotos knipsen könntest.", "Och, schade");
                    return;
                }

                await AddUpload(await media.TakePhotoAsync(new StoreCameraMediaOptions
                {
                    SaveToAlbum = true,
                    DefaultCamera = CameraDevice.Rear
                }));
            };
            #endregion


            #region Library button
            var libraryButton = new ImageButton
            {
                Text = "Foto aus Bibliothek auswählen",
                Source = "picture.png",
                HeightRequest = 40,
                ImageHeightRequest = 30,
                ImageWidthRequest = 30,
                Orientation = ImageOrientation.ImageToLeft,
            };
            libraryButton.Clicked += async (sender, args) =>
            {
                var media = Plugin.Media.CrossMedia.Current;
                if (!media.IsPickPhotoSupported)
                {
                    await DisplayAlert("Oje", "Es sieht so aus, wie wenn du auf deinem Gerät keine Fotos auswählen könntest.", "Och, schade");
                    return;
                }
                await AddUpload(await media.PickPhotoAsync());
            };
            #endregion


            #region Layout
            _queueLabel = new Label();
            Padding = new Thickness(10, 10);
            Content = new StackLayout
            {
                Children = {
                    new StackLayout {
                        VerticalOptions = LayoutOptions.StartAndExpand,
                        Children = {
                            new Label
                            {
                                Text = "Willkommen bei Gruppenfoto!"
                            },
                            new Label
                            {
                                Text = "Hier können du und alle anderen Gäste ihre Schnappschüsse zu einem gemeinsamen Event teilen."
                            },
                            _queueLabel
                        }
                    },
                    new StackLayout
                    {
                        VerticalOptions = LayoutOptions.End,
                        Children =
                        {
                            cameraButton,
                            libraryButton
                        }
                    }
                }
            };
            #endregion

            MessagingCenter.Subscribe<UploadFinishedMessage>(this, "UploadFinished", message => {
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

            Settings.UploadQueue = Settings.UploadQueue.Concat(new[] {fileName}).ToArray();

            UpdateQueueLabel();
        }


        public void UpdateQueueLabel()
        {
            if (Settings.UploadQueue.Length == 0)
            {
                _queueLabel.Text = "";
            }
            else if (Settings.UploadQueue.Length == 1)
            {
                _queueLabel.Text = "1 Foto wartet auf Upload ...";
            }
            else
            {
                _queueLabel.Text = Settings.UploadQueue.Length + " Fotos warten auf Upload ...";
            }
        }
    }
}
