using System;
using System.Collections.Generic;
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

            //string targetNic = GetNetworkInterfaces();

            StartRtmpWatcherThread("");
        }

        private static void StartRtmpWatcherThread(string targetNic)
        {
            var watcher = new RtmpWatcherInterop();

            var thread = new Thread(() => watcher.Start(targetNic, 21935, PacketHandler.OnPacketFound));

            thread.Start();
            thread.Join();
        }

        private static string GetNetworkInterfaces()
        {
            int deviceCount = 0;

            var dic = new Dictionary<int, string>();

            var count = 0;
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                count++;

                // ignore types that can't be instantiated by pcap
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                {
                    continue;
                }

                if (nic.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
                {
                    continue;
                }

                dic[count] = nic.Description;
                Console.WriteLine(string.Format("{3}) Name {1}\r\n\tId {0}\r\n\tDescription {2}\r\n\tType {4}\r\n",
                    nic.Id, nic.Name, nic.Description, ++deviceCount, nic.NetworkInterfaceType));
            }

            
            Console.WriteLine(string.Format("Select device (1 - {0})", deviceCount));

            int choice = Convert.ToInt32(Console.ReadLine());

            return dic[choice];
        }
    }
}
