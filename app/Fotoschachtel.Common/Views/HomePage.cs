using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Fotoschachtel.Common.ViewModels;
using Plugin.Media.Abstractions;
using Refractored.XamForms.PullToRefresh;
using Xamarin.Forms;

namespace Fotoschachtel.Common.Views
{
    public class HomePage : ContentPage
    {
        private Image _settingsButton;

        public HomePage()
        {
            BackgroundColor = Colors.BackgroundColor;
            Padding = new Thickness(0);

            Appearing += async (sender, args) =>
            {
                await ShowLaunchMessage();
            };

            var layout = new AbsoluteLayout();
            BuildMiddleContent(layout);
            BuildTopContent(layout);
            BuildBottomContent(layout);
            Content = layout;
        }


        #region Top
        private void BuildTopContent(AbsoluteLayout layout)
        {
            _settingsButton = Controls.Image("settings.png", 40, async image =>
            {
                await Navigation.PushModalAsync(new SettingsPage(async () => await Refresh()), true);
            });
            layout.Children.Add(_settingsButton, new Rectangle(1, 0, 40, 40), AbsoluteLayoutFlags.XProportional);
        }
        #endregion


        #region Middle
        private PicturesGrid _grid;
        private PicturesViewModel _viewModel;
        private PullToRefreshLayout _pullToRefreshLayout;
        private Label _messageLabel;

        private void BuildMiddleContent(AbsoluteLayout layout)
        {
            _pullToRefreshLayout = new PullToRefreshLayout
            {
                Content = new ScrollView
                {
                    Content = _grid = new PicturesGrid(DisplayMessage)
                },
                RefreshCommand = new Command(async () =>
                {
                    _pullToRefreshLayout.IsRefreshing = true;
                    await Refresh();
                }, () => !IsLoading)
            };
            layout.Children.Add(_pullToRefreshLayout, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.SizeProportional);

            _messageLabel = new Label
            {
                HorizontalTextAlignment = TextAlignment.Center,
                TextColor = Colors.FontColor,
                Margin = new Thickness(Device.OnPlatform(50, 50, 75))
            };
            layout.Children.Add(_messageLabel, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.All);

            Appearing += async (sender, args) =>
            {
                if (!_grid.HasContent)
                {
                    await Refresh();
                    UpdateActivityIndicator();
                }
            };
        }

        public void DisplayMessage(FormattedString fs)
        {
            if (fs == null)
            {
                _messageLabel.Text = "";
                _messageLabel.IsVisible = false;
            }
            else
            {
                _messageLabel.IsVisible = true;
                _messageLabel.FormattedText = fs;
            }
        }


        public async Task Refresh()
        {
            if (IsLoading)
            {
                return;
            }

            var watch = new Stopwatch();
            watch.Start();

            IsLoading = true;

            // maybe we have the images cached already, lets display those first
            var cachedImages = Settings.PicturesCache;
            if (cachedImages.ContainsKey(Settings.Event))
            {
                await _grid.Fill(cachedImages[Settings.Event]);
            }
            else
            {
                _grid.DisplayLoadingMessage();
            }

            await ThumbnailsService.UpdateThumbnails();

            // now load the latest images for real
            try
            {
                _viewModel = _viewModel ?? new PicturesViewModel();
                await _viewModel.Fill();
                IsLoading = false;
                _pullToRefreshLayout.IsRefreshing = false;
            }
            catch (Exception ex)
            {
                IsLoading = false;
                _pullToRefreshLayout.IsRefreshing = false;
                await DisplayAlert("Oje", "Fehler beim Laden der Fotos: " + ex.Message, "Och, doof");
                return;
            }

            // update the cache
            cachedImages[Settings.Event] = _viewModel.Pictures;
            Settings.PicturesCache = cachedImages;

            // and update the grid
            await _grid.Fill(_viewModel.Pictures);

            await DisplayAlert("Done", "Loaded in " + watch.ElapsedMilliseconds + "ms", "OK");
        }


        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                _isLoading = value;
                UpdateActivityIndicator();
            }
        }
        #endregion


        #region Bottom
        private int _uploadsPending;

        private void BuildBottomContent(AbsoluteLayout layout)
        {
            const int buttonSize = 60;

            var media = Plugin.Media.CrossMedia.Current;

            var libraryButton = Controls.Image("library.png", buttonSize, async image =>
            {
                AddUpload(await media.PickPhotoAsync());
            });
            libraryButton.IsVisible = media.IsPickPhotoSupported;

            var cameraButton = Controls.Image("camera.png", buttonSize, async image =>
            {
                AddUpload(await media.TakePhotoAsync(new StoreCameraMediaOptions
                {
                    SaveToAlbum = true,
                    DefaultCamera = CameraDevice.Rear
                }));
            });
            cameraButton.IsVisible = media.IsTakePhotoSupported;

            MessagingCenter.Subscribe<UploadFinishedMessage>(this, "UploadFinished", async message =>
            {
                _uploadsPending--;
                await Refresh();
            });

            layout.Children.Add(libraryButton, new Rectangle(0, 1, buttonSize, buttonSize), AbsoluteLayoutFlags.YProportional);
            layout.Children.Add(cameraButton, new Rectangle(1, 1, buttonSize, buttonSize), AbsoluteLayoutFlags.PositionProportional);
        }


        private void UpdateActivityIndicator()
        {
            if (IsLoading || _grid.IsLoading || _uploadsPending > 0)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        while (_settingsButton != null && (IsLoading || _grid.IsLoading || _uploadsPending > 0))
                        {
                            if (_grid.IsLoading)
                            {
                                await _settingsButton.RelRotateTo(-7, 30, Easing.Linear);
                            }
                            else
                            {
                                await _settingsButton.RelRotateTo(7, 30, Easing.Linear);
                            }
                        }
                    }
                    catch
                    {
                        // do nothing here
                    }
                });
            }
        }


        private void AddUpload(MediaFile file)
        {
            if (file == null)
            {
                return;
            }
            _uploadsPending++;
            UpdateActivityIndicator();

            Settings.UploadQueue = Settings.UploadQueue.Concat(new[] { file.Path }).ToArray();
            MessagingCenter.Send(new StartUploadMessage(), "StartUpload");
        }
        #endregion


        private async Task ShowLaunchMessage()
        {
            if (Settings.LastRunDateTime == null)
            {
                var selectEventPage = new SelectEventPage(async () => await Refresh());
                await Navigation.PushModalAsync(selectEventPage);
            }
            Settings.LastRunDateTime = DateTime.UtcNow;
        }
    }
}
