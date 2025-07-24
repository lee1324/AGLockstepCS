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
            // Start the logging service
            Logger.Start();
            
            
            new TcpServerClientTest().RunTest();

            while (true) {
                Thread.Sleep(33);
            }

            Logger.Info("All tests completed. Press any key to stop servers...");
            Console.ReadKey();
        }
    }
} 