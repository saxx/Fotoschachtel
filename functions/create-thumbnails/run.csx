#r "Microsoft.WindowsAzure.Storage"
#r "System.Drawing"


using System.Net;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using ImageProcessor.Imaging;
using ImageProcessor.Imaging.Formats;
using System.Drawing; 


public static async Task<HttpResponseMessage> Run(HttpRequestMessage request, TraceWriter log)
{
    log.Info($"Triggered by request URI '{request.RequestUri}'.");

    string blob = request.GetQueryNameValuePairs()
        .FirstOrDefault(x => string.Compare(x.Key, "blob", true) == 0)
        .Value;
        
    if (string.IsNullOrEmpty(blob)) {
        log.Info("Parameter 'blob' missing. Exiting.");
        return request.CreateResponse(HttpStatusCode.BadRequest, "Parameter 'blob' is missing.");
    }
    
    await RenderThumbnails(blob, log);

    return request.CreateResponse(HttpStatusCode.OK, ""); 
}


private static CloudBlobContainer GetContainer(string blob)
{
    var storageContainer = "fotoschachtel";
    var storageKey = "11F9B5B/bM5BtfJ2pO5MontYMmygl+ooODdDFHOlnITNlvvbsCDkvscYndAiej/CcsvQmudqu8sNN36i7CBIYQ==";
    
    var storageAccount = new CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(storageContainer, storageKey), true);
    var storageClient = storageAccount.CreateCloudBlobClient(); 
    return storageClient.GetContainerReference(blob);
}


public static async Task RenderThumbnails(string blob, TraceWriter log)
{
    var container = GetContainer(blob);
    if (!await container.ExistsAsync())
    {
        log.Info($"Blob '{blob}' does not exist. Exiting.");
        return;
    }

    var existingFiles = container.ListBlobs(useFlatBlobListing: true).OfType<CloudBlockBlob>().Select(b => b.Name).ToList();
    var existingPrimaryFiles = existingFiles.Where(x => !x.StartsWith("thumbnail")).ToList();
    foreach (var f in existingPrimaryFiles)
    {
        var smallThumbnailExists = existingFiles.Any(x => x == "thumbnails-small/" + f);
        var mediumThumbnailExists = existingFiles.Any(x => x == "thumbnails-medium/" + f);

        if (!smallThumbnailExists || !mediumThumbnailExists)
        {
            log.Info($"Thumbnail missing for file '{f}'.");
			
			await RenderThumbnail(container, f, 200, "thumbnails-small/" + f);
			await RenderThumbnail(container, f, 1000, "thumbnails-medium/" + f);
		
        }
    }
}


private static async Task RenderThumbnail(CloudBlobContainer container, string sourceFileName, int size, string targetFileName)
{
    var sourceBlob = container.GetBlockBlobReference(sourceFileName);
    var targetBlob = container.GetBlockBlobReference(targetFileName);
    
    using (var sourceStream = new MemoryStream()) {
        await sourceBlob.DownloadToStreamAsync(sourceStream);
        sourceStream.Seek(0, SeekOrigin.Begin);
        
        using (var targetStream = await targetBlob.OpenWriteAsync())
        {
            using (var imageFactory = new ImageProcessor.ImageFactory())
            {
                imageFactory
                    .Load(sourceStream)
                    .AutoRotate()
                    .Resize(new ResizeLayer(new Size(size, size), ResizeMode.Max))
                    .Format(new JpegFormat())
                    .Quality(90)
                    .Save(targetStream); 
            } 
        }
    }
}