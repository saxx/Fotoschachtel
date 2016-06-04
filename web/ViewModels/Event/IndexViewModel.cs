using System.Threading.Tasks;
using Fotoschachtel.Services;
using JetBrains.Annotations;

namespace Fotoschachtel.ViewModels.Event
{
    public class IndexViewModel
    {
        private readonly SasService _sasService;
        private readonly HashService _hashService;

        public IndexViewModel(
            [NotNull] SasService sasService,
            [NotNull] HashService hashService)
        {
            _sasService = sasService;
            _hashService = hashService;
        }


        [ItemNotNull]
        public async Task<IndexViewModel> Fill([NotNull] EventMetadata eventMetadata)
        {
            Event = eventMetadata.Event;
            PasswordHash = _hashService.HashEventPassword(eventMetadata.Event, eventMetadata.Password);
            SasToken = await _sasService.GetSasForContainer(eventMetadata.Event, eventMetadata.ContainerName);
            return this;
        }

 
        public SasService.SasToken SasToken { get; set; }
        public string Event { get; set; }
        public string PasswordHash { get; set; }
    }
}
