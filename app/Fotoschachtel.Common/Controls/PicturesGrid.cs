using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fotoschachtel.Common.ViewModels;
using Fotoschachtel.Common.Views;
using Xamarin.Forms;

namespace Fotoschachtel.Common
{
    public class PicturesGrid : Grid
    {
        private readonly Action<FormattedString> _displayMessageCallback;

        public PicturesGrid(Action<FormattedString> displayMessageCallback)
        {
            _displayMessageCallback = displayMessageCallback;
            ColumnSpacing = 1;
            RowSpacing = 1;
        }

        private GalleryPage _galleryPage;
        private int _imagesToLoadCount;
        private IList<PicturesViewModel.Picture> _currentPictures;

        public async Task Fill(IEnumerable<PicturesViewModel.Picture> pictures)
        {
            var picturesList = pictures.ToList();

            // don't refresh the grid if there are no new images
            // this avoids the flashing blue screen
            if (_currentPictures != null && picturesList.Count == _currentPictures.Count && picturesList.All(x => _currentPictures.Contains(x)))
            {
                return;
            }

            _currentPictures = picturesList;

            if (pictures == null || !picturesList.Any())
            {
                DisplayNoPicturesMessage();
                return;
            }

            DisplayLoadingMessage();

            var pictureViews = new List<Image>();
            foreach (var picture in picturesList)
            {
                // build the image
                var image = new Image
                {
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
                            if (_imagesToLoadCount <= 0)
                            {
                                HideMessage();
                            }
                        }
                    }
                };

                // listen for tap events on pictures
                var tapGestureRecognizer = new TapGestureRecognizer();
                var pictureIndexClosure = pictureViews.Count;
                tapGestureRecognizer.Tapped += async (s, e) =>
                {
                    await OpenPicture(pictureIndexClosure);
                };
                image.GestureRecognizers.Add(tapGestureRecognizer);
                pictureViews.Add(image);
            }

            _imagesToLoadCount = picturesList.Count;

            // add the image to the grid
            ColumnDefinitions.Clear();
            RowDefinitions.Clear();
            for (var i = 0; i < pictureViews.Count; i++)
            {
                if (i % 3 == 0)
                {
                    RowDefinitions.Add(new RowDefinition
                    {
                        Height = ImageHeight
                    });
                }
                Children.Add(pictureViews[i], i % 3, i / 3);
            }

            _galleryPage = _galleryPage ?? new GalleryPage();
            await _galleryPage.Build(picturesList);
        }

        public bool IsLoading => _imagesToLoadCount > 0;
        public bool HasContent => _currentPictures != null && _currentPictures.Any();


        private async Task OpenPicture(int pictureIndex)
        {
            await _galleryPage.Open(Navigation, pictureIndex);
        }


        private void DisplayNoPicturesMessage()
        {
            var fs = new FormattedString();
            fs.Spans.Add(new Span { Text = "\n\n\n\nOje, es gibt noch gar keine Fotos im Event " });
            fs.Spans.Add(new Span { Text = Settings.Event, FontAttributes = FontAttributes.Bold });
            fs.Spans.Add(new Span { Text = ".\n\nDu solltest unbedingt gleich welche knipsen!" });
            _displayMessageCallback(fs);
        }


        public void DisplayLoadingMessage()
        {
            var fs = new FormattedString();
            fs.Spans.Add(new Span { Text = "\n\n\n\nEs werden gerade alle Fotos für das Event " });
            fs.Spans.Add(new Span { Text = Settings.Event, FontAttributes = FontAttributes.Bold });
            fs.Spans.Add(new Span { Text = " geladen.\n\nBitte noch einen Moment Geduld ..." });
            _displayMessageCallback(fs);
        }


        private void HideMessage()
        {
            _displayMessageCallback(null);
        }

        private int ImageHeight => Device.OnPlatform(100, 100, 150);
    }
}
