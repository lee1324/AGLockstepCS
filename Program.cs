using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using AGSyncCS;
using SimpleJson;

namespace AGSyncCS
{
    class Program
    {
        static void Main(string[] args)
        {
            
            //RoomsManager.Instance.init(); no more need in local wifi


            // Toggle to enable/disable UDP server
            bool enableUdpServer = false; // Set to false to disable UDP server
            bool startTcpServer = true; // Set to false to skip starting TCP server
            // Start the logging service
            Logger.Start();
            
            if(startTcpServer){
                new TCP_Server().start();
            }

            // Start UDP server in a background thread (toggle)
            if (enableUdpServer) {
                Thread udpServerThread = new Thread(() => {
                    UdpServer udpServer = new UdpServer(Config.UDP_SERVER_PORT, Config.UDP_ECHO_ENABLED);
                    udpServer.Start();
                    
                    // Keep UDP server running
                    while (true) Thread.Sleep(1000);
                });

                udpServerThread.IsBackground = true;
                udpServerThread.Start();
                Logger.Info(string.Format("UDP server started on port {0}", Config.UDP_SERVER_PORT));
            }
            
            // Give servers a moment to start
            Thread.Sleep(3000);
            
            TcpServerClientTest.RunTest();

            while (true) {
                // Keep the main thread alive to allow servers to run
                Thread.Sleep(1000);
            }
            
            Logger.Info("All tests completed. Press any key to stop servers...");
            Console.ReadKey();
            
            Logger.Stop();
        }
    }
} 