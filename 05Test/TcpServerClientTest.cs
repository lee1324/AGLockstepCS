using System;
using System.Threading;
using SimpleJson;

namespace AGSyncCS
{
    public class TcpServerClientTest
    {
        public static void RunTest()
        {
            Logger.Info("=== TCP Server & Client Test ===");

            // Give server a moment to ensure it's ready
            Thread.Sleep(1000);
            Test_InLocalWifi();

            //TestSingleClient();

            //Thread.Sleep(1000);

            //Test_CM_NewRoom();

            //Thread.Sleep(1000);

            // Test multiple clients
            //TestMultipleClients();

            //Thread.Sleep(1000);

            // Test connection limits
            //TestConnectionLimits();

            //Thread.Sleep(1000);
            Logger.Info("=== TCP Server & Client Test Complete ===");
        }

         private static void Test_InLocalWifi()
         {
            Logger.Info("--- Testing Test_InLocalWifi---");
            var localClients = new Client[Config.MaxPlayersPerRoom];
            //owner is clients[0], ignore localClients[0]

            const string roomID = "007024";//for test in A285

            for (int i = 1; i < localClients.Length; ++i) {
                var idx = i;
                Thread clientThread = new Thread(() => {
                    var client = new Client();
                    client.pos = idx;
                    client.roomID = roomID;
                    client.nickname = "TestUser" + idx; // Set a nickname for the client

                    var ownerIP = Tools.RoomID2IP(roomID);
                    //Step 03: client connects to server by RoomID(IP)
                    client.Connect(ownerIP, Config.TCP_SERVER_PORT);
                    localClients[idx] = client;
                });
                clientThread.IsBackground = true;
                clientThread.Start();
                Logger.Debug(string.Format("C Test_InLocalWifi Client {0} started", i));

            }
            Thread.Sleep(1000);

            Logger.Debug("--- Test EnterRoom ---");
             for (int i = 1; i < localClients.Length; ++i)
             {
                 int clientId = i;
                 try {
                     Client client = localClients[clientId];
                     var cm = new CM_EnterRoom();

                     cm.pos = client.pos; // Set position for the client
                     cm.roomID = client.roomID;
                     cm.nickname = client.nickname; // Set a nickname for the client

                    cm.onResponse = (s) => {
                         Logger.Debug("C NewRoom Response:" + s.ToString());//enter success
                     };
                     client.Send(cm);
                 }
                 catch (Exception ex) {
                     Logger.Error(string.Format("ClientId {0} Error: {1}", clientId, ex.Message));
                 }
                 Thread.Sleep(1000);
                 TCP_Server.Instance.localRoom.printState();
            }

             Logger.Debug("--- Test QuitRoom ---");
             for (int i = 1; i < localClients.Length; ++i)
             {
                 int clientId = i;
                 try {
                     Client client = localClients[clientId];
                     var cm = new CM_QuitRoom();

                     cm.pos = client.pos; // Set position for the client
                     cm.roomID = client.roomID;
                     cm.onResponse = (s) => {
                         Logger.Debug("C QuitRoom Response:" + s.ToString());//enter success
                     };
                     client.Send(cm);
                 }
                 catch (Exception ex) {
                     Logger.Error(string.Format("ClientId {0} Error: {1}", clientId, ex.Message));
                 }
                 Thread.Sleep(1000);
                 TCP_Server.Instance.localRoom.printState();
             }

             //Logger.Debug("--- Test Heartbeat ---");
        }
        
        private static void TestSingleClient()
        {
            var cm = new CM_Test();
            cm.i1 = 1001;
            cm.str1 = "CM_Test Hello from client.";
            cm.onResponse = (response) => {
                var sm = (SM_Test)response;
                Logger.Info("response() SM_TEST:" + sm.ToString());
            };
            cm.send();
        }
        
        private static void TestMultipleClients()
        {
            Logger.Info("--- Testing Multiple TCP Clients ---");
            
            // Start multiple clients simultaneously
            for (int i = 1; i <= 5; i++)
            {
                int clientId = i;
                Thread clientThread = new Thread(() =>
                {
                    try
                    {
                        Client client = new Client();
                        client.Connect(Config.TCP_HOST, Config.TCP_SERVER_PORT);
                        
                        for (int j = 1; j <= 2; j++)
                        {
                            var cm = new CM_Test();
                            cm.i1 = 10*i + j;
                            cm.str1 = string.Format("clientID:{0} j:{1}", clientId, j);
                            cm.onResponse = (s) => {
                                var sm = (SM_Test)s;
                                if(sm.i1 - cm.i1 == 20) 
                                    Logger.Info("C Test Correct:" + sm.ToString());
                                else Logger.Info("C Test Wrong:" + sm.ToString());
                            };
                            client.Send(cm);
                        }
                        
                        //client.Close();//lstodo close will cause exception(onServer?)
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(string.Format("ClientId {0} Error: {1}", clientId, ex.Message));
                    }
                });
                clientThread.IsBackground = true;
                clientThread.Start();
            }
            
            // Wait for all clients to complete
            Thread.Sleep(3000);
        }
        
       
        private static void Test_CM_NewRoom()
        {
            Logger.Info("--- Testing Multiple TCP Clients ---");
             for (int i = 0; i < Config.MaxRooms + 10 ; ++i)
            {
                int clientId = i;
                Thread clientThread = new Thread(() =>
                {
                    try
                    {
                        Client client = new Client();
                        client.Connect(Config.TCP_HOST, Config.TCP_SERVER_PORT);

                        
                        var cm = new CM_NewRoom();
                        cm.userID = "autoTestUserID:" + i;
                        cm.onResponse = (s) => {
                            Logger.Debug("C NewRoom Response:" + s.ToString());
                        };
                        client.Send(cm);
                        
                        //client.Close();//lstodo close will cause exception(onServer?)
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(string.Format("ClientId {0} Error: {1}", clientId, ex.Message));
                    }
                });
                clientThread.IsBackground = true;
                clientThread.Start();
                Thread.Sleep(1000);

            }
            
            // Wait for all clients to complete
            Thread.Sleep(3000);
        }
            
        private static void TestConnectionLimits()
        {
            Logger.Info("--- Testing Connection Limits ---");
            
            // Try to create more connections than the server allows
            for (int i = 1; i <= 12; i++) // Server limit is 10
            {
                int clientId = i;
                Thread clientThread = new Thread(() =>
                {
                    try
                    {
                        Client client = new Client();
                        client.Connect(Config.TCP_HOST, Config.TCP_SERVER_PORT);
                        
                        Logger.Info(string.Format("Connection {0} established successfully", clientId));
                        
                        // Keep connection alive briefly
                        Thread.Sleep(Config.TEST_DELAY_MS);
                        
                        client.Close();
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning(string.Format("Connection {0} failed (expected if over limit): {1}", clientId, ex.Message));
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