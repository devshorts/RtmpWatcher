using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RtmpWatcherNet.Parser
{
    public class Container
    {
        public String Name { get; set; }
        public Object Value { get; set; }
        public List<Container> Values { get; set; }
    }

    public class Item
    {
        public String Key { get; set; }
        public String Value { get; set; }
    }
}
