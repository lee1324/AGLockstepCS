using System;
using System.Threading;

namespace AGSyncCS
{
    public class UdpServerTest
    {
        public static void RunTest()
        {
            Logger.Start();
            Logger.Info("=== UDP Server Test ===");
            UdpServer udpServer = new UdpServer(9002, true); // Echo back enabled
            udpServer.Start();

            Logger.Info("UDP server is running on port 9002. Send UDP packets to this port.");
            Logger.Info("Press any key to stop the UDP server...");
            Console.ReadKey();

            udpServer.Stop();
            Logger.Info("=== UDP Server Test Complete ===");
            Logger.Stop();
        }
    }
} 