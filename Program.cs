using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AGServer
{
    class Program
    {
        static void Main(string[] args)
        {
            // Start the logging service
            LogService.Instance.Start();
            LogService.Instance.Info("Application started");
            
            // Start HTTP server in a background thread
            Thread httpServerThread = new Thread(() => {
                HttpServer httpServer = new HttpServer(ServerConfig.HTTP_SERVER_PORT);
                httpServer.Start();
                
                // Keep HTTP server running
                while (true)
                {
                    Thread.Sleep(1000);
                }
            });
            httpServerThread.IsBackground = true;
            httpServerThread.Start();
            
            // Start UDP server in a background thread
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
            
            // Start TCP server in a background thread
            Thread tcpServerThread = new Thread(() => {
                TcpServer tcpServer = new TcpServer(ServerConfig.TCP_SERVER_PORT, ServerConfig.TCP_MAX_CONNECTIONS, ServerConfig.TCP_CONNECTION_TIMEOUT);
                tcpServer.Start();
                
                // Keep TCP server running
                while (true)
                {
                    Thread.Sleep(1000);
                }
            });
            tcpServerThread.IsBackground = true;
            tcpServerThread.Start();
            
            LogService.Instance.Info(string.Format("HTTP server started on port {0}", ServerConfig.HTTP_SERVER_PORT));
            LogService.Instance.Info(string.Format("UDP server started on port {0}", ServerConfig.UDP_SERVER_PORT));
            LogService.Instance.Info(string.Format("TCP server started on port {0}", ServerConfig.TCP_SERVER_PORT));
            LogService.Instance.Info("All servers are running. Press any key to stop...");
            
            // Give servers a moment to start
            Thread.Sleep(3000);
            
            // Run comprehensive server test
            LogService.Instance.Info("=== Starting Comprehensive Server Test ===");
            ComprehensiveServerTest.RunTest();
            
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