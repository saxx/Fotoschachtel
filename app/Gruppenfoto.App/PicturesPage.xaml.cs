using Gruppenfoto.App.ViewModels;
using Xamarin.Forms;

namespace Gruppenfoto.App
{
    public partial class PicturesPage : ContentPage
    {
        public PicturesPage()
        {
            InitializeComponent();
            BindingContext = new PicturesViewModel();

            PicturesList.ItemTapped += (sender, args) =>
            {
                DisplayAlert("Tapped", "Picture Tapped", "Mkay");
            };

            // already load the list
            Refresh();
        }

        private PicturesViewModel ViewModel => BindingContext as PicturesViewModel;

        public void Refresh()
        {
            PicturesList.BeginRefresh();
        }
    }
}
