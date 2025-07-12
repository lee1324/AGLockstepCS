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
            
            LogService.Instance.Info(string.Format("HTTP server started on port {0}", ServerConfig.HTTP_SERVER_PORT));
            LogService.Instance.Info(string.Format("UDP server started on port {0}", ServerConfig.UDP_SERVER_PORT));
            
            // Give servers a moment to start
            Thread.Sleep(2000);
            
            // Run TCP server and client test
            TcpServerClientTest.RunTest();
            
            LogService.Instance.Info("Test completed. Press any key to stop servers...");
            Console.WriteLine("Test completed. Press any key to stop servers...");
            Console.ReadKey();
            
            LogService.Instance.Info("Application finished");
            LogService.Instance.Stop();
        }
    }
} 