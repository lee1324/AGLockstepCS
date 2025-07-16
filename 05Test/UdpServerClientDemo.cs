using System;
using System.Threading;

namespace AGSyncCS
{
    public class UdpServerClientDemo
    {
        public static void RunDemo()
        {
            LogService.Instance.Info("=== UDP Server + Client Demo ===");
            
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
                LogService.Instance.Info("\n--- Synchronous Test ---");
                string response1 = client.SendAndReceive("Hello UDP Server!");
                LogService.Instance.Info("Response: " + response1);

                LogService.Instance.Info("\n--- Multiple Messages Test ---");
                string[] messages = { "Test 1", "Test 2", "Test 3" };
                foreach (string msg in messages)
                {
                    string response = client.SendAndReceive(msg);
                    LogService.Instance.Info(string.Format("Message: {0} -> Response: {1}", msg, response));
                    Thread.Sleep(500);
                }

                LogService.Instance.Info("\n--- Async Test ---");
                client.SendAsync("Async message", (response) =>
                {
                    LogService.Instance.Info("Async response: " + response);
                });
                Thread.Sleep(2000);

                LogService.Instance.Info("\n--- Timeout Test ---");
                string timeoutResponse = client.SendAndReceive("Timeout test", 1000);
                LogService.Instance.Info("Timeout response: " + timeoutResponse);
            }
            catch (Exception ex)
            {
                LogService.Instance.Error("Error: " + ex.Message);
            }
            finally
            {
                client.Close();
            }

            // Wait for the server thread to finish
            serverThread.Join();
            LogService.Instance.Info("=== UDP Server + Client Demo Complete ===");
        }
    }
} 