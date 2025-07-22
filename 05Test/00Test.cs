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
            Logger.Info("--- Testing Test_InLocalWifi ---");
            Logger.Info("--- Master Slaves Mode ---");

            var clients = new BandClient[Config.MaxPlayersPerRoom];
            //owner is clients[0], ignore localClients[0]

            const string roomID = "007024";//for test in A285

            Logger.Info("--- client starts one by one ---");
            for (int i = 0; i < clients.Length; ++i) {
                clients[i] = new BandClient(roomID, i).start();
                
                Logger.Info(string.Format("C Test_InLocalWifi Client {0} started", i));
                Thread.Sleep(100);
             }

            for (int i = 0; i < clients.Length; ++i) {
                clients[i].onPush(Protocals.StartLoading, (sm) => {
                    Logger.Info("todo:房主点开始了，成员开始加载曲谱:" + sm);
                });
            }
            Thread.Sleep(1000);

            Logger.Debug("\n");
            Logger.Debug("--- Test EnterRoom ---");
             for (int i = 0; i < clients.Length; ++i) {
                 int clientId = i;
                 try {
                     var client = clients[clientId];
                    //Step 03: client join the room
                     var cm = new CM_EnterRoom();

                     cm.pos = client.pos; // Set position for the client
                     cm.roomID = client.roomID;
                     cm.nickname = client.nickname; // Set a nickname for the client

                     cm.onResponse = (s) => {
                         Logger.Debug("C NewRoom Response:" + s.ToString());//enter success
                     };
                     client.send(cm);
                 }
                 catch (Exception ex) {
                     Logger.Error(string.Format("ClientId {0} Error: {1}", clientId, ex.Message));
                 }
                 Thread.Sleep(1000);
                 BandServer.Instance.room.printState();
             }


             Logger.Debug("\n");
             Logger.Debug("--- Test QuitRoom ---");
             for (int i = 1; i < clients.Length; ++i)
             {
                 int clientId = i;
                 try {
                     var client = clients[clientId];
                     var cm = new CM_QuitRoom();

                     cm.pos = client.pos; // Set position for the client
                     cm.roomID = client.roomID;
                     cm.onResponse = (s) => {
                         Logger.Debug("C QuitRoom Response:" + s.ToString());//enter success
                     };
                     client.send(cm);
                 }
                 catch (Exception ex) {
                     Logger.Error(string.Format("ClientId {0} Error: {1}", clientId, ex.Message));
                 }
                 Thread.Sleep(1000);
                 BandServer.Instance.room.printState();
             }

             Logger.Debug("\n");
             Logger.Debug("--- Test EnterRoom Again ---");
             for (int i = 1; i < clients.Length; ++i)
             {
                 int clientId = i;
                 try {
                     var client = clients[clientId];
                    //Step 03: client join the room
                     var cm = new CM_EnterRoom();

                     cm.pos = client.pos; // Set position for the client
                     cm.roomID = client.roomID;
                     cm.nickname = client.nickname; // Set a nickname for the client

                    cm.onResponse = (s) => {
                         Logger.Debug("C NewRoom Response:" + s.ToString());//enter success
                     };
                     client.send(cm);
                 }
                 catch (Exception ex) {
                     Logger.Error(string.Format("ClientId {0} Error: {1}", clientId, ex.Message));
                 }
                 Thread.Sleep(1000);
                 BandServer.Instance.room.printState();
            }

            Thread.Sleep(1000);
            BandServer.Instance.notifyStartLoading();
            Thread.Sleep(1000);
            for(int i = 0; i < clients.Length; ++i){
                var client = clients[i];
                Thread t = new Thread(() => {
                    int progress0_100 = 0;
                    while(progress0_100 <= 100) {
                        ++progress0_100;

                        var cm = new CM_LoadingProgress();
                        cm.pos = client.pos;
                        cm.progress0_100 = progress0_100;

                        cm.onResponse = (resp) => {
                            var sm = (SM_LoadingProgress)resp;
                            Logger.Info("todo Refresh UI Progress Now " + sm.getTotalProgress0_1f());
                            if (sm.allCompleted) {
                                Logger.Info("Loading completed for all clients.");
                            }
                        };

                        client.send(cm);
                        Thread.Sleep(30);
                    }
                });
                t.IsBackground = true;
                t.Start();
            }

            if (true) {
                var c = clients[0];
                if (true) {
                    CM_Sync cm = new CM_Sync();
                    cm.pos = 0;
                    cm.syncData = "test as dateTime:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    cm.onResponse = (resp) => {
                        Logger.Info("C Sync Response:" + resp.ToString());
                    };
                    c.send(cm);
                }
                Thread.Sleep(20000);
            }

             //Logger.Debug("--- Test Heartbeat ---");
        }
        
       
    }
} 