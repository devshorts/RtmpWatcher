using System;
using System.IO;
using System.Linq;
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
            using (var stream = new MemoryStream(obj.GetBytes()))
            {
                foreach (var i in Enumerable.Range(0, 9))
                {
                    stream.ReadByte();
                }

                try
                {
                    var o = AMFSerializerUtil<object>.DeserializeAmf(stream);
                }
                catch (Exception ex)
                {

                }

            }
            Console.WriteLine("Found packet from {0} length {1}", obj.GetSourceIP(), obj.GetLength());
        }
    }
}
