using System;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using RtmpInterop;
using RtmpWatcherNet.Common;

namespace RtmpWatcherNet
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Network devices:");

            int deviceCount = GetNetworkInterfaces();

            Console.WriteLine(string.Format("Select device (1 - {0})", deviceCount));

            int choice = Convert.ToInt32(Console.ReadLine());

            StartRtmpWatcherThread(choice);
        }

        private static void StartRtmpWatcherThread(int deviceChoice)
        {
            var watcher = new RtmpWatcherInterop();

            var thread = new Thread(() => watcher.Start(deviceChoice, 21935, OnPacketFound));

            thread.Start();
            thread.Join();
        }

        private static void OnPacketFound(RtmpPacketInterop obj)
        {
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

        private static int GetNetworkInterfaces()
        {
            int deviceCount = 0;
            
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                // ignore types that can't be instantiated by pcap
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                {
                    continue;
                }

                if (nic.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
                {
                    continue;
                }
                
                Console.WriteLine(string.Format("{3}) Name {1}\r\n\tId {0}\r\n\tDescription {2}\r\n\tType {4}\r\n",
                    nic.Id, nic.Name, nic.Description, ++deviceCount, nic.NetworkInterfaceType));
            }

            return deviceCount;
        }
    }
}
