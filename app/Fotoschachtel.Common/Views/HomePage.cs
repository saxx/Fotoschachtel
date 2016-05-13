﻿using System;
using System.IO;
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
        private SettingsPage _settingsPage;

        public HomePage()
        {
            BackgroundColor = Colors.BackgroundColor;
            Padding = Device.OnPlatform(new Thickness(0, 20, 0, 0), new Thickness(0), new Thickness(0));

            var layout = new AbsoluteLayout();
            BuildMiddleContent(layout);
            BuildTopContent(layout);
            BuildBottomContent(layout);
            Content = layout;
        }


        #region Top
        private void BuildTopContent(AbsoluteLayout layout)
        {
            const int buttonSize = 40;

            var settingsButton = Controls.Image("settings.png", buttonSize, async image =>
            {
                await Navigation.PushModalAsync(_settingsPage = _settingsPage ?? new SettingsPage(this), true);
            });

            layout.Children.Add(settingsButton, new Rectangle(1, 0, 40, 40), AbsoluteLayoutFlags.XProportional);
        }
        #endregion


        #region Middle
        private Grid _grid;
        private PicturesViewModel _viewModel;
        private GalleryPage _galleryPage;
        private bool _isLoading;
        private int _imagesToLoadCount;
        private PullToRefreshLayout _pullToRefreshLayout;

        private void BuildMiddleContent(AbsoluteLayout layout)
        {
            _pullToRefreshLayout = new PullToRefreshLayout();
            var scrollView = new ScrollView();

            _grid = new Grid
            {
                ColumnSpacing = 1,
                RowSpacing = 1
            };
            DisplayLoadingMessage();

            scrollView.Content = new StackLayout
            {
                Children =
                {
                    _grid
                }
            };

            _pullToRefreshLayout.Content = scrollView;
            _pullToRefreshLayout.RefreshCommand = new Command(async () => { await RefreshInternal(); }, () => !_isLoading);
            layout.Children.Add(_pullToRefreshLayout, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.SizeProportional);

            Refresh();

            Appearing += (sender, args) =>
            {
                UpdateActivityIndicator();
            };
        }


        public void Refresh()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await RefreshInternal();
            });
        }


        private async Task RefreshInternal()
        {
            if (_isLoading)
            {
                return;
            }

            _isLoading = true;
            UpdateActivityIndicator();

            await ThumbnailsService.UpdateThumbnails();

            try
            {
                _viewModel = _viewModel ?? new PicturesViewModel();
                await _viewModel.Fill();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Oje", "Fehler beim Laden der Fotos: " + ex.Message, "Och, doof");
                _isLoading = false;
                UpdateActivityIndicator();
                return;
            }

            _grid.ColumnDefinitions.Clear();
            _grid.Children.Clear();

            // no pictures, lets display a info message
            if (!_viewModel.Pictures.Any())
            {
                DisplayNoPicturesMessage();
                _isLoading = false;
                UpdateActivityIndicator();
                return;
            }

            _imagesToLoadCount = _viewModel.Pictures.Count();

            var rowCount = 0;
            var columnCount = 0;
            var pictureIndex = -1;
            foreach (var picture in _viewModel.Pictures)
            {
                pictureIndex++;

                // build the image
                var image = new Image
                {
                    MinimumHeightRequest = 120,
                    Aspect = Aspect.AspectFill,
                    Source = new UriImageSource
                    {
                        CacheValidity = TimeSpan.FromDays(30),
                        Uri = new Uri(picture.SmallThumbnailUrl)
                    }
                };

                // listen for property changes, because we need to know when loading is finished
                image.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == "IsLoading")
                    {
                        var senderImage = (Image)sender;
                        if (!senderImage.IsLoading)
                        {
                            _imagesToLoadCount--;
                        }

                        if (_imagesToLoadCount <= 0)
                        {
                            _isLoading = false;
                            UpdateActivityIndicator();
                        }
                    }
                };

                // listen for tap events on pictures
                var tapGestureRecognizer = new TapGestureRecognizer();
                var pictureIndexClosure = pictureIndex;
                tapGestureRecognizer.Tapped += async (s, e) =>
                {
                    await OpenPicture(pictureIndexClosure);
                };
                image.GestureRecognizers.Add(tapGestureRecognizer);

                // add the image to the grid
                _grid.Children.Add(image, columnCount, rowCount);
                if (++columnCount == 3)
                {
                    rowCount++;
                    columnCount = 0;
                }
            }

            _galleryPage = _galleryPage ?? new GalleryPage();
            _galleryPage.Build(_viewModel);
        }



        private async Task OpenPicture(int pictureIndex)
        {
            await _galleryPage.Open(Navigation, pictureIndex);
        }


        private void DisplayNoPicturesMessage()
        {
            var fs = new FormattedString();
            fs.Spans.Add(new Span { Text = "\n\n\n\n Oh, es gibt noch gar\n keine Fotos im Event " });
            fs.Spans.Add(new Span { Text = Settings.Event, FontAttributes = FontAttributes.Bold });
            fs.Spans.Add(new Span { Text = ".\n Du solltest unbedingt gleich welche knipsen!" });

            _grid.Children.Add(new Label
            {
                HorizontalTextAlignment = TextAlignment.Center,
                TextColor = Colors.FontColor,
                FormattedText = fs
            }, 0, 0);
            _isLoading = false;
        }


        private void DisplayLoadingMessage()
        {
            var fs = new FormattedString();
            fs.Spans.Add(new Span { Text = "\n\n\n\n Herzlich willkommen bei Fotoschachtel!\n Wir sind schon dabei, alle Fotos für das Event " });
            fs.Spans.Add(new Span { Text = Settings.Event, FontAttributes = FontAttributes.Bold });
            fs.Spans.Add(new Span { Text = " zu laden.\n Bitte noch einen Moment Geduld ..." });

            _grid.Children.Add(new Label
            {
                HorizontalTextAlignment = TextAlignment.Center,
                TextColor = Colors.FontColor,
                FormattedText = fs
            }, 0, 0);
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

            MessagingCenter.Subscribe<UploadFinishedMessage>(this, "UploadFinished", message =>
            {
                _uploadsPending--;
                Refresh();
            });

            layout.Children.Add(libraryButton, new Rectangle(0, 1, buttonSize, buttonSize), AbsoluteLayoutFlags.YProportional);
            layout.Children.Add(cameraButton, new Rectangle(1, 1, buttonSize, buttonSize), AbsoluteLayoutFlags.PositionProportional);
        }


        private void UpdateActivityIndicator()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                _pullToRefreshLayout.IsRefreshing = _isLoading || _imagesToLoadCount > 0 || _uploadsPending > 0;
            });
        }


        private void AddUpload(MediaFile file)
        {
            if (file == null)
            {
                return;
            }
            _uploadsPending++;
            UpdateActivityIndicator();

            var fileName = Guid.NewGuid() + ".jpg";
            DependencyService.Get<ITemporaryPictureStorage>().Save(fileName, file.GetStream());
            file.Dispose();

            Settings.UploadQueue = Settings.UploadQueue.Concat(new[] { fileName }).ToArray();
            MessagingCenter.Send(new StartUploadMessage(), "StartUpload");
        }
        #endregion

    }
}