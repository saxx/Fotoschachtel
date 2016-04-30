using Foundation;
using Xamarin.Forms;

namespace Gruppenfoto.App.iOS
{
    public class UploaderDelegate : NSUrlSessionTaskDelegate
    {
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
