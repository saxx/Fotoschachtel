using System;
using System.Threading.Tasks;
using Fotoschachtel.Common.ViewModels;
using Xamarin.Forms;

namespace Fotoschachtel.Common.Views
{
    public class GalleryPage : CarouselPage
    {
        private readonly TapGestureRecognizer _tapRecognizer = new TapGestureRecognizer();

        public GalleryPage()
        {
            _tapRecognizer.NumberOfTapsRequired = 2;
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

                page.Padding = Device.OnPlatform(new Thickness(0, 20, 0, 0), new Thickness(0, 0, 0, 0), new Thickness(0, 0, 0, 0));

                var layout = new AbsoluteLayout();
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
                    HorizontalTextAlignment = TextAlignment.Center,
                    Text = GetRelativeDateText(picture.DateTime),
                    BackgroundColor = Color.Transparent,
                    TextColor = Color.White
                };
                var closeButton = Controls.Image("cancel.png", 30, async clickedButton =>
                {
                    await Navigation.PopModalAsync(true);
                });

                layout.GestureRecognizers.Add(_tapRecognizer);

                imageContainer.Content = image;
                layout.Children.Add(imageContainer, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.SizeProportional);
                layout.Children.Add(label, new Rectangle(0.5, 0, 0.5, 30), AbsoluteLayoutFlags.WidthProportional | AbsoluteLayoutFlags.XProportional);
                layout.Children.Add(closeButton, new Rectangle(1, 0, 30, 30), AbsoluteLayoutFlags.XProportional);

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
