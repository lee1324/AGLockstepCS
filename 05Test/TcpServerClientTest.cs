using System;
using System.Threading;
using SimpleJson;

namespace AGSyncCS
{
    public class TcpServerClientTest
    {
        public static void RunTest()
        {
            LogService.Instance.Info("=== TCP Server & Client Test ===");
            
            // Give server a moment to ensure it's ready
            Thread.Sleep(1000);
            
            // Test single client
            TestSingleClient();
            
            // Test multiple clients
            TestMultipleClients();
            
            // Test connection limits
            TestConnectionLimits();
            
            LogService.Instance.Info("=== TCP Server & Client Test Complete ===");
        }
        
        private static void TestSingleClient()
        {
            LogService.Instance.Info("--- Testing Single TCP Client ---");
            
            try
            {
                TcpClientWrapper client = new TcpClientWrapper();
                client.Connect(ServerConfig.TCP_SERVER_ADDRESS, ServerConfig.TCP_SERVER_PORT);
                
                // Test 1: Simple send and receive
                LogService.Instance.Info("Test 1: Simple Send and Receive");
                var js = new JsonObject();
                js["msg"] = "Hello TCP Server!";
                string response1 = client.SendAndReceive("/test", js);
                LogService.Instance.Info("Response 1: " + response1);
                
                // Test 2: Multiple messages
                LogService.Instance.Info("Test 2: Multiple Messages");
                string[] messages = { "Message 1", "Message 2", "Message 3" };
                foreach (string msg in messages)
                {
                    var jsMsg = new JsonObject();
                    jsMsg["msg"] = msg;
                    string response = client.SendAndReceive("/test", jsMsg);
                    LogService.Instance.Info("Message: " + msg + " -> Response: " + response);
                    Thread.Sleep(500);
                }
                
                // Test 3: Long message
                LogService.Instance.Info("Test 3: Long Message");
                string longMessage = "This is a longer message to test TCP server handling of larger data packets. " +
                                   "It contains multiple sentences and should be properly echoed back by the server.";
                var jsLongMessage = new JsonObject();
                jsLongMessage["msg"] = longMessage;
                string longResponse = client.SendAndReceive("/test", jsLongMessage);
                LogService.Instance.Info("Long Response: " + longResponse);
                
                // Test 4: Async send
                LogService.Instance.Info("Test 4: Async Send");
                var jsAsyncMessage = new JsonObject();
                jsAsyncMessage["msg"] = "Async message";
                client.SendAsync("/test", jsAsyncMessage, (response) =>
                {
                    LogService.Instance.Info("Async Response: " + response);
                });
                Thread.Sleep(1000);
                
                client.Close();
                
            }
            catch (Exception ex)
            {
                LogService.Instance.Error("Single Client Test Error: " + ex.Message);
            }
        }
        
        private static void TestMultipleClients()
        {
            LogService.Instance.Info("--- Testing Multiple TCP Clients ---");
            
            // Start multiple clients simultaneously
            for (int i = 1; i <= 3; i++)
            {
                int clientId = i;
                Thread clientThread = new Thread(() =>
                {
                    try
                    {
                        TcpClientWrapper client = new TcpClientWrapper();
                        client.Connect(ServerConfig.TCP_SERVER_ADDRESS, ServerConfig.TCP_SERVER_PORT);
                        
                        for (int j = 1; j <= 2; j++)
                        {
                            var jsMessage = new JsonObject();
                            jsMessage["msg"] = string.Format("Client {0} - Message {1}", clientId, j);
                            string response = client.SendAndReceive("/test", jsMessage);
                            LogService.Instance.Info(string.Format("Client {0}: {1} -> {2}", clientId, jsMessage["msg"], response));
                            Thread.Sleep(ServerConfig.TEST_DELAY_MS);
                        }
                        
                        client.Close();
                    }
                    catch (Exception ex)
                    {
                        LogService.Instance.Error(string.Format("Client {0} Error: {1}", clientId, ex.Message));
                    }
                });
                clientThread.IsBackground = true;
                clientThread.Start();
            }
            
            // Wait for all clients to complete
            Thread.Sleep(3000);
        }
        
       
        
        private static void TestConnectionLimits()
        {
            LogService.Instance.Info("--- Testing Connection Limits ---");
            
            // Try to create more connections than the server allows
            for (int i = 1; i <= 12; i++) // Server limit is 10
            {
                int clientId = i;
                Thread clientThread = new Thread(() =>
                {
                    try
                    {
                        TcpClientWrapper client = new TcpClientWrapper();
                        client.Connect(ServerConfig.TCP_SERVER_ADDRESS, ServerConfig.TCP_SERVER_PORT);
                        
                        LogService.Instance.Info(string.Format("Connection {0} established successfully", clientId));
                        
                        // Keep connection alive briefly
                        Thread.Sleep(ServerConfig.TEST_DELAY_MS);
                        
                        client.Close();
                    }
                    catch (Exception ex)
                    {
                        LogService.Instance.Warning(string.Format("Connection {0} failed (expected if over limit): {1}", clientId, ex.Message));
                    }
                });
                clientThread.IsBackground = true;
                clientThread.Start();
                
                Thread.Sleep(ServerConfig.TEST_DELAY_MS); // Small delay between connection attempts
            }
            
            // Wait for all connection attempts to complete
            Thread.Sleep(3000);
        }
    }
} 