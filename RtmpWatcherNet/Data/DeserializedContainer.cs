using System.Collections.Generic;

namespace RtmpWatcherNet.Data
{
    class DeserializedContainer
    {
        public List<PropertyContainer> Properties { get; set; }
        public List<DeserializedContainer> Objects { get; set; }
    }
}
