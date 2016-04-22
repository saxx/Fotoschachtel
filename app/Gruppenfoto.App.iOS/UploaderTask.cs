using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foundation;
using PCLStorage;
using FileAccess = System.IO.FileAccess;

namespace Gruppenfoto.App.iOS
{
    public class UploaderTask
    {
        private NSUrlSession _session;

        public async Task Start()
        {
            if (_session == null)
            {
                using (var config = NSUrlSessionConfiguration.CreateBackgroundSessionConfiguration("GruppenfotoUpload"))
                {
                    config.HttpMaximumConnectionsPerHost = 4; //iOS Default is 4
                    config.TimeoutIntervalForRequest = 600.0; //30min allowance; iOS default is 60 seconds.
                    config.TimeoutIntervalForResource = 120.0; //2min; iOS Default is 7 days
                    _session = NSUrlSession.FromConfiguration(config, new UploaderDelegate(), new NSOperationQueue());
                }
            }


            while (Settings.UploadQueue.Any())
            {
                var nextFileName = Settings.UploadQueue.First();
                Settings.UploadQueue = Settings.UploadQueue.Skip(1).ToArray();

                var boundary = "FileBoundary";
                // TODO: This is problematic, because there will remain a lot of tmp files that won't get deleted
                var bodyPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Guid.NewGuid() + ".tmp");

                // Create request
                var uploadHandleUrl = NSUrl.FromString($"{Settings.BackendUrl.Trim('/')}/event/{Settings.Event}/picture/");
                var request = new NSMutableUrlRequest(uploadHandleUrl)
                {
                    HttpMethod = "POST",
                    ["Content-Type"] = "multipart/form-data; boundary=" + boundary,
                    ["FileName"] = Path.GetFileName(nextFileName)
                };

                // Construct the body
                var sb = new StringBuilder("");
                sb.AppendFormat("--{0}\r\n", boundary);
                sb.AppendFormat("Content-Disposition: form-data; name=\"file\"; filename=\"{0}\"\r\n", Path.GetFileName(nextFileName));
                sb.Append("Content-Type: application/octet-stream\r\n\r\n");

                if (File.Exists(bodyPath))
                {
                    File.Delete(bodyPath);
                }

                var imageFile = await FileSystem.Current.LocalStorage.GetFileAsync(nextFileName);
                byte[] imageBytes;
                using (var stream = await imageFile.OpenAsync(PCLStorage.FileAccess.Read))
                {
                    imageBytes = new byte[stream.Length];
                    await stream.ReadAsync(imageBytes, 0, imageBytes.Length);
                }

                // Write file to BodyPart
                using (var writeStream = new FileStream(bodyPath, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    writeStream.Write(Encoding.Default.GetBytes(sb.ToString()), 0, sb.Length);
                    writeStream.Write(imageBytes, 0, imageBytes.Length);

                    sb.Clear();
                    sb.AppendFormat("\r\n--{0}--\r\n", boundary);
                    writeStream.Write(Encoding.Default.GetBytes(sb.ToString()), 0, sb.Length);
                }

                // Creating upload task
                var uploadTask = _session.CreateUploadTask(request, NSUrl.FromFilename(bodyPath));
                Console.WriteLine("New TaskID: {0}", uploadTask.TaskIdentifier);

                // Start task
                uploadTask.Resume();
            }
        }
    }
}
