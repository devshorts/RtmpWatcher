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

            var thread = new Thread(() => watcher.Start(deviceChoice, 21935, PacketHandler.OnPacketFound));

            thread.Start();
            thread.Join();
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
