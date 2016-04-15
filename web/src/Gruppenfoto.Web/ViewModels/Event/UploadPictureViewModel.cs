using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Http;

namespace Gruppenfoto.Web.ViewModels.Event
{
    public class UploadPictureViewModel
    {

        public IFormFile File { get; set; }
        public string FileBase64 { get; set; }
    }
}
