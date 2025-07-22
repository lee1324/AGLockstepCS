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
                new BandServer().start();
            }
            
            // Give servers a moment to start
            Thread.Sleep(3000);
            
            TcpServerClientTest.RunTest();

            Logger.Info("All tests completed. Press any key to stop servers...");
            Console.ReadKey();
            
            Logger.Stop();
        }
    }
} 