using System;
using System.Threading;
using Servers;
using SimpleJson;

namespace AGSyncCS
{
    public class TcpServerClientTest
    {
        public static void RunTest()
        {
            Logger.Instance.Info("=== TCP Server & Client Test ===");

            CM.InitConnection();
            // Give server a moment to ensure it's ready
            Thread.Sleep(1000);

            TestSingleClient();

            Thread.Sleep(1000);


            // Test multiple clients
            TestMultipleClients();

            Thread.Sleep(1000);

            // Test connection limits
            //TestConnectionLimits();

            Thread.Sleep(1000);
            Logger.Instance.Info("=== TCP Server & Client Test Complete ===");
        }
        
        private static void TestSingleClient()
        {
            var cm = new CM_Test();
            cm.i1 = 1001;
            cm.str1 = "Hello from client.";
            cm.onResponse = (response) => {
                var sm = (SM_Test)response;
                Logger.Instance.Info("sm_test:" + sm.ToString());
            };
            cm.send();
        }
        
        private static void TestMultipleClients()
        {
            Logger.Instance.Info("--- Testing Multiple TCP Clients ---");
            
            // Start multiple clients simultaneously
            for (int i = 1; i <= 5; i++)
            {
                int clientId = i;
                Thread clientThread = new Thread(() =>
                {
                    try
                    {
                        TcpClientWrapper client = new TcpClientWrapper();
                        client.Connect(Config.TCP_HOST, Config.TCP_SERVER_PORT);
                        
                        for (int j = 1; j <= 2; j++)
                        {
                            var cm = new CM_Test();
                            cm.i1 = 10*i + j;
                            cm.str1 = string.Format("clientID:{0} j:{1}", clientId, j);
                            cm.onResponse = (s) => {
                                var sm = (SM_Test)s;
                                if(sm.i1 - cm.i1 == 20) 
                                    Logger.Instance.Info("C Test Correct:" + sm.ToString());
                                else Logger.Instance.Info("C Test Wrong:" + sm.ToString());
                            };
                            client.Send(cm);
                        }
                        
                        //client.Close();//lstodo close will cause exception(onServer?)
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.Error(string.Format("ClientId {0} Error: {1}", clientId, ex.Message));
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
            Logger.Instance.Info("--- Testing Connection Limits ---");
            
            // Try to create more connections than the server allows
            for (int i = 1; i <= 12; i++) // Server limit is 10
            {
                int clientId = i;
                Thread clientThread = new Thread(() =>
                {
                    try
                    {
                        TcpClientWrapper client = new TcpClientWrapper();
                        client.Connect(Config.TCP_HOST, Config.TCP_SERVER_PORT);
                        
                        Logger.Instance.Info(string.Format("Connection {0} established successfully", clientId));
                        
                        // Keep connection alive briefly
                        Thread.Sleep(Config.TEST_DELAY_MS);
                        
                        client.Close();
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.Warning(string.Format("Connection {0} failed (expected if over limit): {1}", clientId, ex.Message));
                    }
                });
                clientThread.IsBackground = true;
                clientThread.Start();
                
                Thread.Sleep(Config.TEST_DELAY_MS); // Small delay between connection attempts
            }
            
            // Wait for all connection attempts to complete
            Thread.Sleep(3000);
        }
    }
} 