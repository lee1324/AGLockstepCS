using System;
using System.Threading;

namespace AGServer
{
    public class UdpClientTest
    {
        public static void RunTest()
        {
            LogService.Instance.Start();
            LogService.Instance.Info("=== UDP Client Test ===");
            
            UdpClientWrapper client = new UdpClientWrapper();
            
            try
            {
                // Connect to the UDP server
                client.Connect("127.0.0.1", 9002);
                
                // Test synchronous send and receive
                LogService.Instance.Info("\n--- Synchronous Test ---");
                string response1 = client.SendAndReceive("Hello UDP Server!");
                LogService.Instance.Info("Response: " + response1);
                
                // Test multiple messages
                LogService.Instance.Info("\n--- Multiple Messages Test ---");
                string[] messages = { "Test 1", "Test 2", "Test 3" };
                foreach (string msg in messages)
                {
                    string response = client.SendAndReceive(msg);
                    LogService.Instance.Info(string.Format("Message: {0} -> Response: {1}", msg, response));
                    Thread.Sleep(500); // Small delay between messages
                }
                
                // Test async send
                LogService.Instance.Info("\n--- Async Test ---");
                client.SendAsync("Async message", (response) =>
                {
                    LogService.Instance.Info("Async response: " + response);
                });
                
                // Wait a bit for async response
                Thread.Sleep(2000);
                
                // Test timeout
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
            
            LogService.Instance.Info("=== UDP Client Test Complete ===");
            LogService.Instance.Stop();
        }
    }
} 