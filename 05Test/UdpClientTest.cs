//using System;
//using System.Threading;

//namespace AGSyncCS
//{
//    public class UdpClientTest
//    {
//        public static void RunTest()
//        {
//            Logger.Start();
//            Logger.Info("=== UDP Client Test ===");
            
//            UdpClientWrapper client = new UdpClientWrapper();
            
//            try
//            {
//                // Connect to the UDP server
//                client.Connect("127.0.0.1", 9002);
                
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
//                Thread.Sleep(2000); // Wait for async response
                
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
//                // UdpServer server = new UdpServer(Config.UDP_SERVER_PORT); // This line was removed as per the new_code
//                client.Close();
//                Logger.Info("=== UDP Client Test Complete ===");
//                Logger.Stop();
//            }
//        }
//    }
//} 