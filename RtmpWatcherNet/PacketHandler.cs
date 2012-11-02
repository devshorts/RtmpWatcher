using System;
using System.IO;
using RtmpInterop;
using RtmpWatcherNet.Common;

namespace RtmpWatcherNet
{
    class PacketHandler
    {
        public static void OnPacketFound(RtmpPacketInterop obj)
        {
            return;
            using (var newStream = new MemoryStream(obj.GetBytes()))
            {
                var streamCopy = PruneStreamChunkDelimiters(newStream, obj);

                Console.WriteLine("Found packet {2} from {0} length {1}", obj.GetSourceIP(), obj.GetLength(),
                                  obj.GetRtmpPacketType());


                var decoder = AMFSerializerUtil<object>.DeserializeAmf(streamCopy);


                Console.WriteLine();
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
