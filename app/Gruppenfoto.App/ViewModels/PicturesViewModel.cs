using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using PCLStorage;
using Xamarin.Forms;

namespace Gruppenfoto.App.ViewModels
{
    public class PicturesViewModel : BaseViewModel
    {
        private ObservableCollection<Picture> _pictures = new ObservableCollection<Picture>();
        public ObservableCollection<Picture> Pictures
        {
            get { return _pictures; }
            set { _pictures = value; OnPropertyChanged(); }
        }


        public string EventId { get; private set; }


        private bool _hasNoPictures = true;
        public bool HasNoPictures
        {
            get { return _hasNoPictures; }
            set { _hasNoPictures = value; OnPropertyChanged(); }
        }


        private Command _loadPicturesCommand;
        public Command LoadPicturesCommand
        {
            get
            {
                return _loadPicturesCommand ?? (_loadPicturesCommand = new Command(ExecuteLoadPicturesCommand, () => !IsBusy));
            }
        }


        private async void ExecuteLoadPicturesCommand()
        {
            if (IsBusy)
            {
                return;
            }

            EventId = Settings.EventId;
            IsBusy = true;
            LoadPicturesCommand.ChangeCanExecute();
            try
            {
                string responseString;
                using (var httpClient = new System.Net.Http.HttpClient())
                {
                    responseString = await httpClient.GetStringAsync($"{Settings.BackendUrl.Trim('/')}/event/{EventId}.json");
                }

                var responseContent = Newtonsoft.Json.JsonConvert.DeserializeObject<PicturesResponse>(responseString);
                var pictures = responseContent.Pictures.OrderByDescending(x => x.CreationDateTime).ToList();
                HasNoPictures = !pictures.Any();
                Pictures.Clear();
                foreach (var p in pictures)
                {
                    p.CreationDateTime = p.CreationDateTime.ToLocalTime();

                    var imageFileName = "thumbnail_" + p.FileId + ".jpg";
                    var imageFileExists = await FileSystem.Current.LocalStorage.CheckExistsAsync(imageFileName);
                    if (imageFileExists != ExistenceCheckResult.FileExists)
                    {
                        var imageFile = await FileSystem.Current.LocalStorage.CreateFileAsync(imageFileName, CreationCollisionOption.ReplaceExisting);
                        try
                        {
                            using (var httpClient = new System.Net.Http.HttpClient())
                            {
                                var bytes = await httpClient.GetByteArrayAsync($"{Settings.BackendUrl.Trim('/')}/event/{EventId}/picture/{p.FileId}?size=100");
                                using (var stream = await imageFile.OpenAsync(FileAccess.ReadAndWrite))
                                {
                                    await stream.WriteAsync(bytes, 0, bytes.Length);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            await imageFile.DeleteAsync();
                        }
                    }

                    Pictures.Add(p);
                }
            }
            catch (Exception ex)
            {
                await new ContentPage().DisplayAlert("Oje", "Beim Laden der Fotos ist ein Fehler aufgetreten\n" + ex.Message, "Och, doof");
            }

            IsBusy = false;
            LoadPicturesCommand.ChangeCanExecute();
        }


        public class Picture
        {
            public string FileId { get; set; }
            public DateTime CreationDateTime { get; set; }
            public int ImageBytes { get; set; }
            public ImageSource Image
            {
                get
                {
                    var imageFileName = "thumbnail_" + FileId + ".jpg";
                    var imageFile = FileSystem.Current.LocalStorage.CreateFileAsync(imageFileName, CreationCollisionOption.OpenIfExists).Result;
                    byte[] imageBytes;
                    using (var stream = imageFile.OpenAsync(FileAccess.Read).Result)
                    {
                        imageBytes = new byte[stream.Length];
                        stream.ReadAsync(imageBytes, 0, imageBytes.Length).GetAwaiter().GetResult();
                    }
                    ImageBytes = imageBytes.Length;

                    return ImageSource.FromStream(() => new MemoryStream(imageBytes));
                }
            }
        }


        public class PicturesResponse
        {
            public string EventId { get; set; }
            public IEnumerable<Picture> Pictures { get; set; }
        }
    }
}
