using System;
using RtmpInterop;

namespace RtmpWatcherNet
{
    class Program
    {
        static void Main(string[] args)
        {
            var watcher = new RtmpWatcherInterop();

            watcher.Start(8935);

            Console.ReadKey();
        }
    }
}
