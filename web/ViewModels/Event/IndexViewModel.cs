using System.Threading.Tasks;
using Fotoschachtel.Services;
using JetBrains.Annotations;

namespace Fotoschachtel.ViewModels.Event
{
    public class IndexViewModel
    {
        private readonly SasService _sasService;

        public IndexViewModel([NotNull] SasService sasService)
        {
            _sasService = sasService;
        }


        public async Task<IndexViewModel> Fill([NotNull] string eventName, [NotNull] string containerName)
        {
            Event = eventName;
            SasToken = await _sasService.GetSasForContainer(eventName, containerName);
            return this;
        }
        
        public SasService.SasToken SasToken { get; set; }
        public string Event { get; set; }
    }
}
