using System;
using System.Threading;

namespace AGSyncCS
{
    public class UdpServerClientDemo
    {
        public static void RunDemo()
        {
            Logger.Instance.Info("=== UDP Server + Client Demo ===");
            
            // Start the UDP server in a background thread
            Thread serverThread = new Thread(() => {
                UdpServer server = new UdpServer(9002, true);
                server.Start();
                // Keep the server running for the duration of the demo
                Thread.Sleep(8000);
                server.Stop();
            });
            serverThread.IsBackground = true;
            serverThread.Start();

            // Give the server a moment to start
            Thread.Sleep(1000);

            // Run the UDP client test
            UdpClientWrapper client = new UdpClientWrapper();
            try
            {
                client.Connect("127.0.0.1", 9002);
                Logger.Instance.Info("\n--- Synchronous Test ---");
                string response1 = client.SendAndReceive("Hello UDP Server!");
                Logger.Instance.Info("Response: " + response1);

                Logger.Instance.Info("\n--- Multiple Messages Test ---");
                string[] messages = { "Test 1", "Test 2", "Test 3" };
                foreach (string msg in messages)
                {
                    string response = client.SendAndReceive(msg);
                    Logger.Instance.Info(string.Format("Message: {0} -> Response: {1}", msg, response));
                    Thread.Sleep(500);
                }

                Logger.Instance.Info("\n--- Async Test ---");
                client.SendAsync("Async message", (response) =>
                {
                    Logger.Instance.Info("Async response: " + response);
                });
                Thread.Sleep(2000);

                Logger.Instance.Info("\n--- Timeout Test ---");
                string timeoutResponse = client.SendAndReceive("Timeout test", 1000);
                Logger.Instance.Info("Timeout response: " + timeoutResponse);
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("Error: " + ex.Message);
            }
            finally
            {
                client.Close();
            }

            // Wait for the server thread to finish
            serverThread.Join();
            Logger.Instance.Info("=== UDP Server + Client Demo Complete ===");
        }
    }
} 