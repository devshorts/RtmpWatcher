using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using RtmpInterop;
using RtmpWatcherNet.Common;
using RtmpWatcherNet.Parser;

namespace RtmpWatcherNet
{
    public class PacketHandler
    {
        public static void OnPacketFound(RtmpPacketInterop obj)
        {

            AmfContainer deserializedOutput = null;
//
//            var x = "new byte[]{";
//            foreach(var b in obj.GetBytes())
//            {
//                x += b.ToString("x2") + ",";
//            }
//            x += "};";

            using (var newStream = new MemoryStream(obj.GetBytes()))
            {
                var streamCopy = PruneStreamChunkDelimiters(newStream, obj);

                Console.WriteLine("Found packet {2} from {0} length {1}", obj.GetSourceIP(), obj.GetLength(),
                                  obj.GetRtmpPacketType());


                deserializedOutput = AMFSerializerUtil<AmfContainer>.DeserializeAmf(streamCopy);
            }

            Visualize(deserializedOutput, 0);
        }

        private static void Visualize(AmfContainer container, int count)
        {
            if (container == null)
            {
                return;
            }

            var tabs = Enumerable.Range(0, count).Select(i => "\t").FoldToDelimitedList(i => i, "");
            Console.WriteLine("{0}{1}: {2}", tabs, container.Name, container.Value);

            if(CollectionUtil.IsNullOrEmpty(container.Values))
            {
                return;
            }

            foreach (var value in container.Values)
            {
                Visualize(value, ++count);
            }
        }

        private static MemoryStream PruneStreamChunkDelimiters(MemoryStream newStream, RtmpPacketInterop obj)
        {
            var streamCopy = new MemoryStream(obj.GetLength());
            var count = 1;

            for (int i = 0; i < 9; i++)
            {
                byte b = Convert.ToByte(newStream.ReadByte());
                streamCopy.WriteByte(b);
            }

            while (newStream.Position != obj.GetLength())
            {
                byte b = Convert.ToByte(newStream.ReadByte());

                if (count % 128 == 0 && b == 0xc3)
                {
                }
                else
                {
                    count++;
                    streamCopy.WriteByte(b);
                }
            }

            streamCopy.Seek(0, SeekOrigin.Begin);
            var firstByte = streamCopy.ReadByte();
            bool useLongHeader = firstByte == 0;
            int objectId;
            if (!useLongHeader)
            {
                objectId = firstByte & 0x0F;
            }

            if (useLongHeader)
            {
                streamCopy.Seek(12, SeekOrigin.Begin);
            }
            else
            {
                streamCopy.Seek(8, SeekOrigin.Begin);
            }

            return streamCopy;
        }
    }
}
