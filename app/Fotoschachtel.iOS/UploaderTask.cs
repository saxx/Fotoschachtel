using System.Linq;
using System.Threading.Tasks;
using Fotoschachtel.Common;
using Foundation;
using Xamarin.Forms;

namespace Fotoschachtel.Ios
{
    public class UploaderTask
    {
        private NSUrlSession _session;

        public async Task Start()
        {
            if (_session == null)
            {
                using (var config = NSUrlSessionConfiguration.CreateBackgroundSessionConfiguration("FotoschachtelUpload"))
                {
                    _session = NSUrlSession.FromConfiguration(config, new UploaderDelegate(), new NSOperationQueue());
                }
            }

            var sasToken = await Settings.GetSasToken();

            while (Settings.UploadQueue.Any())
            {
                var nextFileName = Settings.UploadQueue.FirstOrDefault();
                if (nextFileName == null)
                {
                    break;
                }
                Settings.UploadQueue = Settings.UploadQueue.Skip(1).ToArray();

                var uploadHandleUrl = NSUrl.FromString($"{sasToken.ContainerUrl}/{nextFileName}{sasToken.SasQueryString}");
                var request = new NSMutableUrlRequest(uploadHandleUrl)
                {
                    HttpMethod = "PUT",
                    ["x-ms-blob-type"] = "BlockBlob"
                };

                var filePath = DependencyService.Get<ITemporaryPictureStorage>().GetFullPath(nextFileName);
                var uploadTask = _session.CreateUploadTask(request, NSUrl.FromFilename(filePath));
                uploadTask.Resume();
            }
        }
    }
}
