using System;
using System.Linq;
using System.Threading.Tasks;
using Gruppenfoto.App;
using Gruppenfoto.App.ViewModels;
using Refractored.XamForms.PullToRefresh;
using Xamarin.Forms;

namespace Fotoschachtel.Common
{
    public class HomePageMiddleContent : PullToRefreshLayout
    {
        private readonly HomePage _parentPage;
        private readonly Grid _grid;

        private readonly PicturesViewModel _viewModel = new PicturesViewModel();
        private bool _isAlreadyLoaded;
        private bool _isLoading;
        private int _imagesToLoadCount;

        private readonly GalleryPage _galleryPage = new GalleryPage();


        public HomePageMiddleContent(HomePage parentPage)
        {
            _parentPage = parentPage;
            BackgroundColor = Colors.BackgroundColor;
            VerticalOptions = LayoutOptions.CenterAndExpand;

            var scrollView = new ScrollView();

            _grid = new Grid
            {
                ColumnSpacing = 1,
                RowSpacing = 1
            };

            scrollView.Content = new StackLayout
            {
                Children =
                {
                    _grid
                }
            };
            Content = scrollView;

            parentPage.Appearing += async (sender, e) =>
            {
                if (!_isAlreadyLoaded)
                {
                    await Refresh();
                }
            };
            RefreshCommand = new Command(async () => { await Refresh(); }, () => !_isLoading);
        }


        public async Task Refresh()
        {
            if (_isLoading)
            {
                return;
            }

            _isAlreadyLoaded = true;
            IsRefreshing = _isLoading = true;

            await ThumbnailsService.UpdateThumbnails();

            try
            {
                await _viewModel.Fill();
            }
            catch (Exception ex)
            {
                await _parentPage.DisplayAlert("Oje", "Fehler beim Laden der Fotos: " + ex.Message, "Och, doof");
                return;
            }

            _grid.ColumnDefinitions.Clear();
            _grid.Children.Clear();

            // no pictures, lets display a info message
            if (!_viewModel.Pictures.Any())
            {
                DisplayNoPicturesMessage();
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
                image.PropertyChanged += Image_PropertyChanged;

                // listen for tap events on pictures
                var tapGestureRecognizer = new TapGestureRecognizer();
                var pictureIndexClosure = pictureIndex;
                tapGestureRecognizer.Tapped += async (s, e) =>
                {
                    //if (!_isLoading)
                    //{
                    await OpenPicture(pictureIndexClosure);
                    //}
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

            _galleryPage.Build(_viewModel);
        }


        private void Image_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsLoading")
            {
                var image = (Image)sender;
                if (!image.IsLoading)
                {
                    _imagesToLoadCount--;
                }

                if (_imagesToLoadCount <= 0)
                {
                    IsRefreshing = _isLoading = false;
                }
            }
        }


        private async Task OpenPicture(int pictureIndex)
        {
            await _galleryPage.Open(Navigation, pictureIndex);
        }


        private void DisplayNoPicturesMessage()
        {
            var fs = new FormattedString();
            fs.Spans.Add(new Span { Text = "Oh, es gibt noch gar\n keine Fotos im Event " });
            fs.Spans.Add(new Span { Text = Settings.Event, FontAttributes = FontAttributes.Bold });
            fs.Spans.Add(new Span { Text = ".\n Du solltest unbedingt gleich welche knipsen!" });

            _grid.Children.Add(new Label
            {
                HorizontalTextAlignment = TextAlignment.Center,
                TextColor = Colors.FontColor,
                FormattedText = fs
            }, 0, 0);
            IsRefreshing = _isLoading = false;
        }
    }
}
