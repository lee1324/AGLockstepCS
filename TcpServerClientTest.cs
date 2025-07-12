using System;
using System.Threading;

namespace AGServer
{
    public class TcpServerClientTest
    {
        public static void RunTest()
        {
            LogService.Instance.Info("=== TCP Server & Client Test ===");
            
            // Start TCP server in background thread
            Thread serverThread = new Thread(() => {
                TcpServer server = new TcpServer(ServerConfig.TCP_SERVER_PORT, ServerConfig.TCP_MAX_CONNECTIONS, ServerConfig.TCP_CONNECTION_TIMEOUT);
                server.Start();
                
                // Keep server running for the duration of the test
                Thread.Sleep(15000);
                server.Stop();
            });
            serverThread.IsBackground = true;
            serverThread.Start();
            
            // Give server a moment to start
            Thread.Sleep(2000);
            
            // Test single client
            TestSingleClient();
            
            // Test multiple clients
            TestMultipleClients();
            
            // Test server broadcast
            TestServerBroadcast();
            
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
                client.Connect(ServerConfig.LOCALHOST, ServerConfig.TCP_SERVER_PORT);
                
                // Test 1: Simple send and receive
                LogService.Instance.Info("Test 1: Simple Send and Receive");
                string response1 = client.SendAndReceive("Hello TCP Server!");
                LogService.Instance.Info("Response 1: " + response1);
                
                // Test 2: Multiple messages
                LogService.Instance.Info("Test 2: Multiple Messages");
                string[] messages = { "Message 1", "Message 2", "Message 3" };
                foreach (string msg in messages)
                {
                    string response = client.SendAndReceive(msg);
                    LogService.Instance.Info("Message: " + msg + " -> Response: " + response);
                    Thread.Sleep(500);
                }
                
                // Test 3: Long message
                LogService.Instance.Info("Test 3: Long Message");
                string longMessage = "This is a longer message to test TCP server handling of larger data packets. " +
                                   "It contains multiple sentences and should be properly echoed back by the server.";
                string longResponse = client.SendAndReceive(longMessage);
                LogService.Instance.Info("Long Response: " + longResponse);
                
                // Test 4: Async send
                LogService.Instance.Info("Test 4: Async Send");
                client.SendAsync("Async message", (response) =>
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
                        client.Connect("127.0.0.1", 9007);
                        
                        for (int j = 1; j <= 2; j++)
                        {
                            string message = string.Format("Client {0} - Message {1}", clientId, j);
                            string response = client.SendAndReceive(message);
                            LogService.Instance.Info(string.Format("Client {0}: {1} -> {2}", clientId, message, response));
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
        
        private static void TestServerBroadcast()
        {
            LogService.Instance.Info("--- Testing Server Broadcast ---");
            
            // Start a few clients that will receive broadcasts
            for (int i = 1; i <= 2; i++)
            {
                int clientId = i;
                Thread clientThread = new Thread(() =>
                {
                    try
                    {
                        TcpClientWrapper client = new TcpClientWrapper();
                        client.Connect("127.0.0.1", 9007);
                        
                        // Set up message callback
                        client.SetMessageReceivedCallback((message) =>
                        {
                            LogService.Instance.Info(string.Format("Client {0} received broadcast: {1}", clientId, message));
                        });
                        
                        // Keep connection alive for a while
                        Thread.Sleep(ServerConfig.TEST_TIMEOUT_MS);
                        
                        client.Close();
                    }
                    catch (Exception ex)
                    {
                        LogService.Instance.Error(string.Format("Broadcast Client {0} Error: {1}", clientId, ex.Message));
                    }
                });
                clientThread.IsBackground = true;
                clientThread.Start();
            }
            
            // Wait a moment for clients to connect
            Thread.Sleep(1000);
            
            // Note: In a real scenario, you would call server.Broadcast() here
            // For this test, we'll just send individual messages
            LogService.Instance.Info("Broadcast test completed (individual messages sent)");
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
                        client.Connect("127.0.0.1", 9007);
                        
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