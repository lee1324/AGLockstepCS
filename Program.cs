using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SimpleJson;

namespace AGSyncCS
{
    class Program
    {
        static void Main(string[] args)
        {
            // Toggle to enable/disable UDP server
            bool enableUdpServer = false; // Set to false to disable UDP server
            // Start the logging service
            Logger.Instance.Start();
            
            // Start TCP server in a background thread
            Thread tcpServerThread = new Thread(() => {
                TcpServer tcpServer = new TcpServer(ServerConfig.TCP_SERVER_PORT, 
                    ServerConfig.TCP_MAX_CONNECTIONS, ServerConfig.TCP_CONNECTION_TIMEOUT);
                tcpServer.Start();
                
                // Keep TCP server running
                while (true) Thread.Sleep(1000);
            });
            tcpServerThread.IsBackground = true;
            tcpServerThread.Start();
            
            // Start UDP server in a background thread (toggle)
            if (enableUdpServer) {
                Thread udpServerThread = new Thread(() => {
                    UdpServer udpServer = new UdpServer(ServerConfig.UDP_SERVER_PORT, ServerConfig.UDP_ECHO_ENABLED);
                    udpServer.Start();
                    
                    // Keep UDP server running
                    while (true) Thread.Sleep(1000);
                });

                udpServerThread.IsBackground = true;
                udpServerThread.Start();
                Logger.Instance.Info(string.Format("UDP server started on port {0}", ServerConfig.UDP_SERVER_PORT));
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