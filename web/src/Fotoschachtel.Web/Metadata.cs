using System;

namespace Fotoschachtel
{
    public class Metadata
    {
        public EventMetadata[] Events { get; set; }
    }

    public class EventMetadata
    {
        public string Event { get; set; }
        public string Password { get; set; }
        public string ContainerName { get; set; }
        public string CreatorEmail { get; set; }
        public DateTime CreatedDateTime { get; set; }
    }
}
