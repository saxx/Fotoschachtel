using System.Threading.Tasks;
using Xamarin.Forms;

namespace Fotoschachtel.Common
{
    public static class NavigationExtensions
    {
        public static Task<string> Prompt(this INavigation navigation, string title, string description, string currentValue = null)
        {
            // we want to wait in-process until the user finished his input
            var tcs = new TaskCompletionSource<string>();

            var titleLabel = Controls.Label(title);
            titleLabel.HorizontalOptions = LayoutOptions.Center;
            titleLabel.FontAttributes = FontAttributes.Bold;
            var messageLabel = Controls.Label(description);
            messageLabel.Margin = new Thickness(10);
            var textbox = Controls.EntryMonospace(currentValue ?? "");

            var okButton = Controls.Image("save.png", 40, async image =>
            {
                await navigation.PopModalAsync();
                tcs.SetResult(textbox.Text);
            });
            okButton.HorizontalOptions = LayoutOptions.End;
            var cancelButton = Controls.Image("cancel.png", 40, async image =>
            {
                await navigation.PopModalAsync();
                tcs.SetResult(null);
            });
            cancelButton.HorizontalOptions = LayoutOptions.StartAndExpand;

            var buttons = new StackLayout
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Orientation = StackOrientation.Horizontal,
                Children = { cancelButton, okButton },
            };
            var layout = new StackLayout
            {
                Padding = new Thickness(10, 40, 10, 10),
                VerticalOptions = LayoutOptions.StartAndExpand,
                Children = { titleLabel, messageLabel, textbox, buttons }
            };

            // create and show page
            var page = new ContentPage
            {
                Content = layout,
                BackgroundColor = Colors.BackgroundColor
            };
            navigation.PushModalAsync(page);

            // open the keyboard
            textbox.Focus();

            // code waits here until tcs.SetResult is called
            return tcs.Task;
        }
    }
}
