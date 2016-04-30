using System;
using System.Linq;
using System.Threading.Tasks;
using Gruppenfoto.App.ViewModels;
using Xamarin.Forms;

namespace Gruppenfoto.App
{
    public partial class PicturesPage
    {
        private PicturesViewModel ViewModel => BindingContext as PicturesViewModel;
        private bool _isAlreadyLoaded;
        private bool _isLoading;
        private int _imagesToLoadCount;

        private readonly GalleryPage _galleryPage = new GalleryPage();

        public PicturesPage()
        {
            InitializeComponent();
            BindingContext = new PicturesViewModel();

            // we seem to be unable to display the loading icon of the pull-down-control on the first start
            // so we're using another loading indicator for the first launch only
            ActivityIndicator.IsVisible = ActivityIndicator.IsRunning = true;
            Appearing += async (sender, e) =>
            {
                if (!_isAlreadyLoaded)
                {
                    await RefreshInternal();
                }
            };
            PullToRefresh.RefreshCommand = new Command(async () => { await RefreshInternal(); }, () => !_isLoading);
        }


        public void Refresh()
        {
            _isAlreadyLoaded = false;
        }


        private async Task RefreshInternal()
        {
            if (_isLoading)
            {
                return;
            }

            _isAlreadyLoaded = true;
            PullToRefresh.IsRefreshing = _isLoading = true;

            await ThumbnailsService.UpdateThumbnails();

            try
            {
                await ViewModel.Fill();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Oje", "Fehler beim Laden der Fotos: " + ex.Message, "Och, doof");
            }

            Grid.ColumnDefinitions.Clear();
            Grid.Children.Clear();

            // no pictures, lets display a info message
            if (!ViewModel.Pictures.Any())
            {
                DisplayNoPicturesMessage();
                return;
            }

            _imagesToLoadCount = ViewModel.Pictures.Count();

            var rowCount = 0;
            var columnCount = 0;
            var pictureIndex = -1;
            foreach (var picture in ViewModel.Pictures)
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
                Grid.Children.Add(image, columnCount, rowCount);
                if (++columnCount == 3)
                {
                    rowCount++;
                    columnCount = 0;
                }
            }

            _galleryPage.Build(ViewModel);
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
                    PullToRefresh.IsRefreshing = _isLoading = false;
                    ActivityIndicator.IsVisible = ActivityIndicator.IsRunning = false;
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
            fs.Spans.Add(new Span { Text = "Oh, es gibt noch gar keine Fotos im Event " });
            fs.Spans.Add(new Span { Text = Settings.Event, FontAttributes = FontAttributes.Bold });
            fs.Spans.Add(new Span { Text = ". Du solltest unbedingt gleich welche knipsen!" });

            Grid.Children.Add(new Label
            {
                FormattedText = fs
            }, 0, 0);
            PullToRefresh.IsRefreshing = _isLoading = false;
            ActivityIndicator.IsVisible = ActivityIndicator.IsRunning = false;
        }
    }
}
