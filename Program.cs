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
            Logger.Instance.Start();
            
            if(startTcpServer){

                //Step 00: owner starts game server in local wifi.
                Thread tcpServerThread = new Thread(() => {
                    TCP_Server tcpServer = new TCP_Server(Config.TCP_SERVER_PORT, 
                        Config.TCP_MAX_CONNECTIONS, Config.TCP_CONNECTION_TIMEOUT);
                    tcpServer.Start();

                    //Step 01: owner creates a local room
                    tcpServer.newLocalRoom("owner"); // Create a local room
                    // Keep TCP server running
                    while (true) Thread.Sleep(1000);
                });
                tcpServerThread.IsBackground = true;
                tcpServerThread.Start();

                
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
                Logger.Instance.Info(string.Format("UDP server started on port {0}", Config.UDP_SERVER_PORT));
            }
            
            // Give servers a moment to start
            Thread.Sleep(3000);
            
            TcpServerClientTest.RunTest();
            
            Logger.Instance.Info("All tests completed. Press any key to stop servers...");
            Console.ReadKey();
            
            Logger.Instance.Stop();
        }
    }
} 