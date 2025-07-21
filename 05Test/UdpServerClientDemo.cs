//using System;
//using System.Threading;

//namespace AGSyncCS
//{
//    public class UdpServerClientDemo
//    {
//        public static void RunTest()
//        {
//            Logger.Info("=== UDP Server + Client Demo ===");
            
//            // Start server in a background thread
//            UdpServer server = new UdpServer(Config.UDP_SERVER_PORT);
//            server.Start();
//            // Keep the server running for the duration of the demo
//            Thread.Sleep(8000);
//            server.Stop();

//            // Give the server a moment to start
//            Thread.Sleep(1000);

//            // Run the UDP client test
//            UdpClientWrapper client = new UdpClientWrapper();
//            try
//            {
//                // Synchronous test
//                Logger.Info("\n--- Synchronous Test ---");
//                string response1 = client.SendAndReceive("Hello Server (sync)");
//                Logger.Info("Response: " + response1);

//                // Multiple messages test
//                Logger.Info("\n--- Multiple Messages Test ---");
//                for (int i = 0; i < 3; i++)
//                {
//                    string msg = "Message " + i;
//                    string response = client.SendAndReceive(msg, 1000); // 1 sec timeout
//                    Logger.Info(string.Format("Message: {0} -> Response: {1}", msg, response));
//                }

//                // Async test
//                Logger.Info("\n--- Async Test ---");
//                client.SendAndReceiveAsync("Hello Server (async)", (response) => {
//                    Logger.Info("Async response: " + response);
//                });
                
//                // Timeout test
//                Logger.Info("\n--- Timeout Test ---");
//                string timeoutResponse = client.SendAndReceive("Timeout test", 500); // 500 ms timeout
//                Logger.Info("Timeout response: " + timeoutResponse);
//            }
//            catch (Exception ex)
//            {
//                Logger.Error("Error: " + ex.Message);
//            }
//            finally
//            {
//                // Clean up
//                server.Stop();
//                client.Close();

//                Logger.Info("=== UDP Server + Client Demo Complete ===");
//            }
//        }
//    }
//} 