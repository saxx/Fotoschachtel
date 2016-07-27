using System;
using System.Threading.Tasks;
using Fotoschachtel.Common.ViewModels;
using Xamarin.Forms;

namespace Fotoschachtel.Common.Views
{
    public class GalleryPage : CarouselPage
    {
        public void Build(PicturesViewModel viewModel)
        {
            Children.Clear();

            foreach (var picture in viewModel.Pictures)
            {
                var page = new ContentPage
                {
                    BackgroundColor = Color.Black,
                    Padding = new Thickness(0)
                };

                var layout = new AbsoluteLayout();
                var imageContainer = new PinchToZoomContainer();

                var image = new Image
                {
                    Source = new UriImageSource
                    {
                        CacheValidity = TimeSpan.FromDays(30),
                        Uri = new Uri(picture.MediumThumbnailUrl)
                    }
                };
                var label = new Label
                {
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    Text = GetRelativeDateText(picture.DateTime),
                    BackgroundColor = Color.Black,
                    TextColor = Color.White
                };
                var closeButton = Controls.Image("cancel.png", 25, async clickedButton =>
                {
                    await Navigation.PopModalAsync(true);
                });

                imageContainer.Content = image;
                layout.Children.Add(imageContainer, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.SizeProportional);
                layout.Children.Add(label, new Rectangle(0, 0, 1, 25), AbsoluteLayoutFlags.WidthProportional);
                layout.Children.Add(closeButton, new Rectangle(1, 0, 25, 25), AbsoluteLayoutFlags.XProportional);

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
            var difference = DateTime.UtcNow - dateUtc;

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
