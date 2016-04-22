using Foundation;
using Xamarin.Forms;

namespace Gruppenfoto.App.iOS
{
    public class UploaderDelegate : NSUrlSessionTaskDelegate
    {
        // Called by iOS when the task finished trasferring data. It's important to note that his is called even when there isn't an error.
        // See: https://developer.apple.com/library/ios/documentation/Foundation/Reference/NSURLSessionTaskDelegate_protocol/index.html#//apple_ref/occ/intfm/NSURLSessionTaskDelegate/URLSession:task:didCompleteWithError:
        public override void DidCompleteWithError(NSUrlSession session, NSUrlSessionTask task, NSError error)
        {
            MessagingCenter.Send(new UploadFinishedMessage(), "UploadFinished");
        }

        public override void DidFinishEventsForBackgroundSession(NSUrlSession session)
        {
            MessagingCenter.Send(new UploadFinishedMessage(), "UploadFinished");
        }

        public override void DidSendBodyData(NSUrlSession session, NSUrlSessionTask task, long bytesSent, long totalBytesSent, long totalBytesExpectedToSend)
        {
            MessagingCenter.Send(new UploadFinishedMessage(), "UploadFinished");
        }
    }
}
