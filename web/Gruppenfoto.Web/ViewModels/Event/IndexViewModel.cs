using System.Collections.Generic;
using JetBrains.Annotations;

namespace Gruppenfoto.Web.ViewModels.Event
{
    public class IndexViewModel
    {
        #region Constructor
        [NotNull]
        private readonly PictureStorage _storage;

        public IndexViewModel([NotNull] PictureStorage storage)
        {
            _storage = storage;
        }
        #endregion


        [NotNull]
        public IndexViewModel Fill([NotNull] string eventId)
        {
            EventId = eventId;
            Pictures = _storage.Load(eventId);
            return this;
        }


        [NotNull]
        public string EventId { get; set; } = "< unknown event >";

        [NotNull, ItemNotNull]
        public IEnumerable<Picture> Pictures { get; set; } = new List<Picture>();
    }
}
