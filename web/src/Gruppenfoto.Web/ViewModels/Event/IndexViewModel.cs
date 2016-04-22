using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Gruppenfoto.Web.ViewModels.Event
{
    public class IndexViewModel
    {
        private readonly SasService _sasService;

        public IndexViewModel([NotNull] SasService sasService)
        {
            _sasService = sasService;
        }


        public async Task<IndexViewModel> Fill([CanBeNull] string eventId)
        {
            SasToken = await _sasService.GetSasForEvent(eventId);
            return this;
        }
        
        public SasService.SasToken SasToken { get; set; }
    }
}
