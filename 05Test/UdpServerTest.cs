using System;
using System.Threading;

namespace AGSyncCS
{
    public class UdpServerTest
    {
        public static void RunTest()
        {
            LogService.Instance.Start();
            LogService.Instance.Info("=== UDP Server Test ===");
            UdpServer udpServer = new UdpServer(9002, true); // Echo back enabled
            udpServer.Start();

            LogService.Instance.Info("UDP server is running on port 9002. Send UDP packets to this port.");
            LogService.Instance.Info("Press any key to stop the UDP server...");
            Console.ReadKey();

            udpServer.Stop();
            LogService.Instance.Info("=== UDP Server Test Complete ===");
            LogService.Instance.Stop();
        }
    }
} 