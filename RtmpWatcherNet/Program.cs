using System;
using System.Threading;
using RtmpInterop;

namespace RtmpWatcherNet
{
    class Program
    {
        static void Main(string[] args)
        {
            var watcher = new RtmpWatcherInterop();

            var thread = new Thread(() => watcher.Start(8935));

            thread.Start();

            Console.ReadKey();

            watcher.Complete();

            Console.WriteLine("Done!");

            Console.ReadKey();
        }
    }
}
