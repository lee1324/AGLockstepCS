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
            LogService.Instance.Start();
            LogService.Instance.Info("Application started");
            
            // Start TCP server in a background thread
            Thread tcpServerThread = new Thread(() => {
                TcpServer tcpServer = new TcpServer(ServerConfig.TCP_SERVER_PORT, 
                    ServerConfig.TCP_MAX_CONNECTIONS, ServerConfig.TCP_CONNECTION_TIMEOUT);
                // Register a listener for path "/test"
                tcpServer.AddListener("newRoom", node => {
                    LogService.Instance.Info($"[Listener] Received on newRoom: {node}");
                });
                tcpServer.Start();
                
                // Keep TCP server running
                while (true)
                {
                    Thread.Sleep(1000);
                }
            });
            tcpServerThread.IsBackground = true;
            tcpServerThread.Start();
            
            // Start UDP server in a background thread (toggle)
            if (enableUdpServer)
            {
                Thread udpServerThread = new Thread(() => {
                    UdpServer udpServer = new UdpServer(ServerConfig.UDP_SERVER_PORT, ServerConfig.UDP_ECHO_ENABLED);
                    udpServer.Start();
                    
                    // Keep UDP server running
                    while (true)
                    {
                        Thread.Sleep(1000);
                    }
                });
                udpServerThread.IsBackground = true;
                udpServerThread.Start();
                LogService.Instance.Info(string.Format("UDP server started on port {0}", ServerConfig.UDP_SERVER_PORT));
            }
            else
            {
                LogService.Instance.Info("UDP server is disabled by toggle.");
            }
            
            LogService.Instance.Info(string.Format("TCP server started on port {0}", ServerConfig.TCP_SERVER_PORT));
            LogService.Instance.Info("All servers are running. Press any key to stop...");
            
            // Give servers a moment to start
            Thread.Sleep(3000);
            
            // Run comprehensive server test
            //LogService.Instance.Info("=== Starting Comprehensive Server Test ===");
            //ComprehensiveServerTest.RunTest();
            
            // Run TCP server and client test
            LogService.Instance.Info("=== Starting TCP Server & Client Test ===");
            TcpServerClientTest.RunTest();
            
            LogService.Instance.Info("All tests completed. Press any key to stop servers...");
            Console.WriteLine("All tests completed. Press any key to stop servers...");
            Console.ReadKey();
            
            LogService.Instance.Info("Application finished");
            LogService.Instance.Stop();
        }
    }
} 