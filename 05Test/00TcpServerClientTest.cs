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

            var localClients = new Client[Config.MaxPlayersPerRoom];
            //owner is clients[0], ignore localClients[0]

            const string roomID = "007024";//for test in A285

             for (int i = 1; i < localClients.Length; ++i) {
                var client = new Client().start(roomID, i);
                localClients[i] = client;
                Logger.Debug(string.Format("C Test_InLocalWifi Client {0} started", i));

            }
            Thread.Sleep(1000);

            for (int i = 1; i < localClients.Length; ++i) {
                localClients[i].onPush(Protocals.StartLoading, (sm) => {
                    Logger.Debug("成员开始加载曲谱:" + sm);
                });

            }
            Thread.Sleep(1000);

            Logger.Debug("\n");
            Logger.Debug("--- Test EnterRoom ---");
             for (int i = 1; i < localClients.Length; ++i)
             {
                 int clientId = i;
                 try {
                     Client client = localClients[clientId];
                    //Step 03: client join the room
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
                 TCP_Server.Instance.room.printState();
             }


             Logger.Debug("\n");
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
                 TCP_Server.Instance.room.printState();
             }

             Logger.Debug("\n");
             Logger.Debug("--- Test EnterRoom Again ---");
             for (int i = 1; i < localClients.Length; ++i)
             {
                 int clientId = i;
                 try {
                     Client client = localClients[clientId];
                    //Step 03: client join the room
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
                 TCP_Server.Instance.room.printState();
            }

            Thread.Sleep(1000);
            TCP_Server.Instance.notifyStartLoading();
            Thread.Sleep(1000);
            for(int i = 1; i < localClients.Length; ++i){
                var client = localClients[i];
                Thread t = new Thread(() => {
                    var cm = new CM_LoadingProgress();
                    cm.pos = client.pos;
                    cm.progress0_100 = 0;
                    cm.onResponse = (resp) => {
                        var sm = (SM_LoadingProgress)resp;
                        Logger.Info("c " + sm);
                        Logger.Info("todo Refresh UI Progress Now " + sm.getTotalProgress0_1f());
                    };
                    while( cm.progress0_100 <= 100) {
                        ++cm.progress0_100;

                        client.Send(cm);
                        Thread.Sleep(100);
                    }
                });
                t.IsBackground = true;
                t.Start();
            }

             //Logger.Debug("--- Test Heartbeat ---");
        }
        
       
    }
} 