using System.Linq;
using System.Threading.Tasks;
using Fotoschachtel.Common;
using Foundation;

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
                var nextFilePath = Settings.UploadQueue.FirstOrDefault();
                if (nextFilePath == null)
                {
                    break;
                }
                Settings.UploadQueue = Settings.UploadQueue.Skip(1).ToArray();

                var uploadHandleUrl = NSUrl.FromString($"{sasToken.ContainerUrl}/{nextFilePath}{sasToken.SasQueryString}");
                var request = new NSMutableUrlRequest(uploadHandleUrl)
                {
                    HttpMethod = "PUT",
                    ["x-ms-blob-type"] = "BlockBlob"
                };

                var uploadTask = _session.CreateUploadTask(request, NSUrl.FromFilename(nextFilePath));
                uploadTask.Resume();
            }
        }
    }
}
