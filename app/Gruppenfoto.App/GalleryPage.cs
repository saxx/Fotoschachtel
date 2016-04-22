using System;
using System.Threading.Tasks;
using Gruppenfoto.App.ViewModels;
using Xamarin.Forms;

namespace Gruppenfoto.App
{
    public class GalleryPage : CarouselPage
    {
        private readonly TapGestureRecognizer _tapRecognizer = new TapGestureRecognizer();

        public GalleryPage()
        {
            _tapRecognizer.Tapped += async (sender, args) =>
            {
                await Navigation.PopModalAsync(true);
            };
        }


        public void Build(PicturesViewModel viewModel)
        {
            Children.Clear();

            foreach (var picture in viewModel.Pictures)
            {
                var page = new ContentPage
                {
                    BackgroundColor = Color.Black,
                    Padding = new Thickness(0, 20, 0, 0)
                };

                var layout = new StackLayout();
                var imageContainer = new PinchToZoomContainer();

                var image = new Image
                {
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    Source = new UriImageSource
                    {
                        CacheValidity = TimeSpan.FromDays(30),
                        Uri = new Uri(picture.MediumThumbnailUrl)
                    }
                };
                var label = new Label
                {
                    HeightRequest = 30,
                    HorizontalTextAlignment = TextAlignment.Center,
                    Text = GetRelativeDateText(picture.DateTime),
                    BackgroundColor = Color.Transparent,
                    TextColor = Color.White
                };

                layout.GestureRecognizers.Add(_tapRecognizer);

                imageContainer.Content = image;
                layout.Children.Add(label);
                layout.Children.Add(imageContainer);
                page.Content = layout;

                Children.Add(page);
            }
        }


        public async Task Open(INavigation navigation, int pictureIndex)
        {
            if (Children.Count >= pictureIndex)
            {
                CurrentPage = Children[pictureIndex];
            }

            await navigation.PushModalAsync(this, true);
        }


        private string GetRelativeDateText(DateTime dateUtc)
        {
            var difference = (DateTime.UtcNow - dateUtc);

            if (difference.TotalSeconds <= 60)
            {
                return "Gerade eben";
            }
            if (difference.TotalMinutes < 120)
            {
                return $"Vor {difference.TotalMinutes.ToString("N0")} Minuten";
            }
            if (difference.TotalHours < 48)
            {
                return $"Vor {difference.TotalHours.ToString("N0")} Stunden (um {dateUtc.ToLocalTime().ToString("HH:mm")})";
            }
            return $"Vor {difference.TotalDays.ToString("N0")} Tagen (um {dateUtc.ToLocalTime().ToString("HH:mm")})";
        }
    }
}
