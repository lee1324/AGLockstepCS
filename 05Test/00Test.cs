using SimpleJson;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace AGSyncCS
{
    public class TcpServerClientTest
    {

        public static void RunTest()
        {
            Logger.Info("=== TCP Server & Client Test ===");

            // Give server a moment to ensure it's ready
      

            if (false) {
                Logger.Info("--- test port taken ---");
                int takenSize = 5;//ok, max_port_retry +1 = fail
                try{
                    for(int i = 0; i < takenSize; ++i){
                        var ls = new TcpListener(IPAddress.Any, Config.TCP_SERVER_PORT + i);
                        ls.Start();
                    }
                }
                catch (Exception e) {

                }
            }


            BandServers servers = new BandServers();

            for(int i = 0; i < 5; ++i){
                Logger.Info("=== Start Servers Once :" + i + " ===");
                servers.start(() => {
                    Logger.Info(string.Format("Server started successfully portTCP:{0} portUDP:{1} ",
                        servers.portTCP, servers.portUDP));
                     //servers.stop();//不可在start回调中stop，否则会导致服务器无法启动
                }, (errorCode) => {
                    Logger.Error("Failed to start server, error code: " + errorCode);
                });
                Thread.Sleep(500);//give server some time to start
                Logger.Info("=== Stop Servers ===");
                servers.stop();
            }

            //finally start
            servers.start(() => {
                 Logger.Info(string.Format("Server started successfully portTCP:{0} portUDP:{1} ",
                    servers.portTCP, servers.portUDP));
             }, (errorCode) => {
                    Logger.Error("Failed to start server, error code: " + errorCode);
             });
            Thread.Sleep(500);//give server some time to start;


            var clients = new BandClient[Config.MaxPlayersPerRoom];
            //owner is clients[0], ignore localClients[0]

            const string roomID = "007024";//for test in A285

            Logger.Info("--- client starts one by one ---");
            for (int i = 0; i < clients.Length; ++i) {
                var c = new BandClient(roomID, i).start(() => {
                    Logger.Info(string.Format("C Test_InLocalWifi Client {0} started", i));
                });
                clients[i] = c;
                Thread.Sleep(100);
             }

            Thread.Sleep(3000);//give all clients some time to check connection

            Logger.Info("=== Test Complete ===");
            return;

            for (int i = 0; i < clients.Length; ++i) {
                clients[i].onPush(Protocals.StartLoading, (sm) => {
                    Logger.Info("todo:房主点开始了，成员开始加载曲谱:" + sm);
                });
            }
            Thread.Sleep(4000);//give clients some time to retry different ports

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
                 servers.room.printState();
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
                 servers.room.printState();
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
                 servers.room.printState();
            }

            Thread.Sleep(1000);
            servers.tcpServer.notifyStartLoading();
            Thread.Sleep(1000);
            for(int i = 0; i < clients.Length; ++i){
                var client = clients[i];
                Thread t = new Thread(() => {
                    int progress0_100 = 0;
                    while(progress0_100 <= 100) {
                        progress0_100 += 10;

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

            while (true) {
                for(int i = 0; i < clients.Length; ++i) {
                    var c = clients[i];
                    var cm = new CM_Sync();
                    cm.pos = i;
                    cm.syncData = i + " - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    c.dt = DateTime.Now;

                    cm.onResponse = (resp) => {
                        //var info = string.Format("C{0} Sync Response:{1}", c.pos, resp.ToString());
                        //Logger.Info(info);
                        Logger.Info("delay:" + DateTime.Now.Subtract(c.dt).TotalMilliseconds + "ms");
                    };
                    c.send(cm);
                }
                Thread.Sleep(33);
            }

             //Logger.Debug("--- Test Heartbeat ---");
        }
        
       
    }
} 