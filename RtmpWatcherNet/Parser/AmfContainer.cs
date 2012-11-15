using System;
using System.Collections.Generic;

namespace RtmpWatcherNet.Parser
{
    public class AmfContainer
    {
        public String Name { get; set; }
        public Object Value { get; set; }
        public List<AmfContainer> Values { get; set; }
    }
}
