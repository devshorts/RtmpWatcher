using System;
using System.IO;
using System.Linq;
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
            var watcher = new RtmpWatcherInterop();

            var thread = new Thread(() => watcher.Start(21935, OnPacketFound));

            thread.Start();

//            Console.ReadKey();
//
//            watcher.Complete();
//
//            Console.WriteLine("Done!");
//
//            Console.ReadKey();

            thread.Join();
        }

        private static void OnPacketFound(RtmpPacketInterop obj)
        {
            String bytesString = "";
            using (var stream = new MemoryStream(obj.GetBytes()))
            {
                bytesString = Encoding.UTF8.GetString(stream.ToArray());
            }

            Console.WriteLine("Found packet {2} from {0} length {1}", obj.GetSourceIP(), obj.GetLength(), obj.GetRtmpPacketType());
            Console.WriteLine("\t\t{0}", bytesString.Substring(0,bytesString.Length > 200 ? 200 : bytesString.Length));
            Console.WriteLine();
        }
    }
}
