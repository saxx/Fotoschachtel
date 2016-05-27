using System;
using Xamarin.Forms;

namespace Fotoschachtel.Common
{
    public static class Controls
    {
        #region Label
        public static Label Label(string text, string fontFamily, Action<Label> onClick = null)
        {
            var label = new Label
            {
                Text = text,
                BackgroundColor = Colors.BackgroundColor,
                TextColor = Colors.FontColor,
                FontFamily = fontFamily
            };

            if (onClick != null)
            {
                label.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(() => { onClick.Invoke(label); })
                });
            }

            return label;
        }

        public static Label Label(string text, Action<Label> onClick = null)
        {
            return Label(text, Fonts.Normal, onClick);
        }


        public static Label Label(FormattedString text)
        {
            var label = Label("");
            label.FormattedText = text;
            return label;
        }


        public static Label LabelMonospace(string text, Action<Label> onClick = null)
        {
            return Label(text, Fonts.Monospace, onClick);
        }

        public static Label Separator(int height = 20)
        {
            return new Label
            {
                HeightRequest = height
            };
        }
        #endregion


        #region Button
        public static Button Button(string text, string fontFamily, Action<Button> onClick = null)
        {
            var button = new Button
            {
                Text = text,
                BackgroundColor = Colors.FontColor,
                TextColor = Colors.BackgroundColor,
                FontFamily = fontFamily
            };

            if (onClick != null)
            {
                button.Clicked += (sender, args) =>
                {
                    onClick.Invoke(button);
                };
            }

            return button;
        }

        public static Button Button(string text, Action<Button> onClick = null)
        {
            return Button(text, Fonts.Normal, onClick);
        }
        #endregion


        #region Entry
        public static Entry Entry(string text, string fontFamily)
        {
            var entry = new Entry
            {
                Text = text,
                BackgroundColor = Colors.BackgroundColor,
                TextColor = Colors.FontColor,
                FontFamily = fontFamily
            };

            return entry;
        }

        public static Entry Entry(string text)
        {
            return Entry(text, Fonts.Normal);
        }

        public static Entry EntryMonospace(string text)
        {
            return Entry(text, Fonts.Monospace);
        }

        public static Entry Password(string text)
        {
            var entry = Entry(text, Fonts.Normal);
            entry.IsPassword = true;
            return entry;
        }
        #endregion


        #region Image
        public static Image Image(string resourceId, int width, int height, Action<Image> onClick = null)
        {
            if (!resourceId.Contains(".Images."))
            {
                resourceId = "Fotoschachtel.Common.Images." + resourceId;
                if (!resourceId.EndsWith(".png"))
                {
                    resourceId += ".png";
                }
            }

            var image = new Image
            {
                Source = ImageSource.FromResource(resourceId, Type.GetType("Fotoschachtel.Common.App")),
                HeightRequest = height,
                WidthRequest = width
            };

            if (onClick != null)
            {
                image.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(() => { onClick.Invoke(image); })
                });
            }

            return image;
        }

        public static Image Image(string resourceId, int size, Action<Image> onClick = null)
        {
            return Image(resourceId, size, size, onClick);
        }
        #endregion
    }
}
