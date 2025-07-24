using SimpleJson;
using System;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace AGSyncCS
{
    public class TcpServerClientTest
    {
        BandClient[] clients = null;

        public  void RunTest()
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

            if(false) {
                for(int i = 0; i < 2; ++i){
                    Logger.Info("");
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
            }

            //finally start
            servers.start(() => {
                 Logger.Info(string.Format("Server started successfully portTCP:{0} portUDP:{1} ",
                    servers.portTCP, servers.portUDP));
             }, (errorCode) => {
                    Logger.Error("Failed to start server, error code: " + errorCode);
             });
            Thread.Sleep(500);//give server some time to start;


            clients = new BandClient[Config.MaxPlayersPerRoom];

            Logger.Debug("");
            Logger.Debug("=== clients init push listeners ===");
            for (int i = 0; i < clients.Length; ++i) {
                var c = new BandClient();
                c.onPush(Protocals.StartLoading, (sm) => {
                    Logger.Info("todo:房主点开始了，成员开始加载曲谱:" + sm);
                });
                c.onPush(Protocals.TakePos, (sm) => {
                    Logger.Info("有人占位了，当前占位:" + sm);
                });
                c.onPush(Protocals.CancelPos, (sm) => {
                    Logger.Info("有人取消位置了，当前占位:" + sm);
                });
                clients[i] = c;
            }

            Logger.Info("");
            Logger.Info("=== test search rooms ===");
            clients[1].searchRooms((roomsIDs) => {
                for(int i = 0; i < roomsIDs.Length; ++i){
                    Logger.Info(string.Format("    roomdsIDs[{0}]:{1}", i, roomsIDs[i]));
                }
            });

            Thread.Sleep(5000);//give some time to search rooms

            Logger.Info("");
            Logger.Info("=== test clients enter room ===");
            const string roomID = "007024";//for test in A285

            foreach(var c in clients) {
                c.enterRoom(roomID, () => {
                    Logger.Info(string.Format("one Client enter room successfully"));
                });
                Thread.Sleep(100);
            }

            Thread.Sleep(5000);//search rooms needs a lot of time, give all clients some time to check connection

    
            Thread.Sleep(4000);//give clients some time to retry different ports

            Logger.Debug("\n");
            Logger.Debug("--- Test Member1 EnterRoom  ---");//房主不能发送此消息！
             for (int i = 0; i < clients.Length; ++i) {
                 int clientId = i;
                 try {
                     var client = clients[clientId];
                    //Step 03: client join the room
                     var cm = new CM_EnterRoom();
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
                 Thread.Sleep(300);
                 servers.room.printState();
             }


             Logger.Info("");
             Logger.Info("--- Test Member1 QuitRoom  ---");//房主不能发送此消息！
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
                 Thread.Sleep(300);
                 servers.room.printState();
             }

             Logger.Info("");
             Logger.Info("--- Test Member1 EnterRoom Again  ---");//房主不能发送此消息！
             for (int i = 1; i < clients.Length; ++i)
             {
                 int clientId = i;
                 try {
                     var client = clients[clientId];
                    //Step 03: client join the room
                     var cm = new CM_EnterRoom();
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
                 Thread.Sleep(300);
                 servers.room.printState();
            }

            Logger.Info("=== test take pos ===");
            if (true) {
                var c = clients[0];
                var cm = new CM_TakePos();
                cm.pos = 0;
                cm.onResponse = (resp) => {
                    var sm = (SM_TakePos)resp;
                    Logger.Info("TakePos Response: " + sm.ToString());
                };
                Logger.Info("==== client0 发送消息：take pos 0 ====");
                c.send(cm);
                Thread.Sleep(1000);
                //lstodo add global errorCode handler like position_occupied
                Logger.Info("==== client0 发送消息:take pos 0 ====");

                c.send(cm);
                Thread.Sleep(1000);
            }

            for(int i = 0; i < 5; ++i){
                var c = clients[1];
                if (true) {
                    var cm = new CM_TakePos();
                    cm.pos = 1;
                    cm.onResponse = (resp) => {
                        var sm = (SM_TakePos)resp;
                        Logger.Info("TakePos Response: " + sm.ToString());
                    };
                    Logger.Info(string.Format("==== client1 发送消息:take pos 1 ({0}/5)====", i));
                    c.send(cm);
                    Thread.Sleep(1000);
                }
                if (true) {
                    var cm = new CM_CancelPos();
                    cm.pos = 1;
                    cm.onResponse = (resp) => {
                        var sm = (SM_CancelPos)resp;
                        Logger.Info("TakePos Response: " + sm.ToString());
                    };
                    Logger.Info(string.Format("==== client1 发送消息:cancel pos 1 ({0}/5)====", i));
                    c.send(cm);
                    Thread.Sleep(1000);
                }
            }

            if (true) {
                var c = clients[1];
                var cm = new CM_TakePos();
                cm.pos = 1;
                cm.onResponse = (resp) => {
                    var sm = (SM_TakePos)resp;
                    Logger.Info("TakePos Response: " + sm.ToString());
                    c.pos = sm.pos;//Important!
                };
                Logger.Info("==== client1 发送消息:take pos 1 ====");
                c.send(cm);
                Thread.Sleep(1000);
            }

            servers.tcpServer.notifyStartLoading();
            Thread.Sleep(1000);
            for(int i = 0; i < clients.Length; ++i){
                var client = clients[i];
                Thread t = new Thread(() => {
                    int progress0_100 = 0;
                    while(progress0_100 <= 100) {
                        progress0_100 += 10;
                        if (progress0_100  > 100) progress0_100 = 100;

                        var cm = new CM_LoadingProgress();
                        cm.pos = client.pos;
                        cm.progress0_100 = progress0_100;

                        cm.onResponse = (resp) => {
                            var sm = (SM_LoadingProgress)resp;
                            Logger.Info("总进度(所有人)：" + sm.getTotalProgress0_1f());
                            if (sm.allCompleted) {
                                Logger.Info("所有前端进load完，房主要发送同步数据了！");
                                syncTest();
                            }
                        };

                        client.send(cm);
                        Thread.Sleep(100);
                    }
                });
                t.Start();
            }

     
            Logger.Info("");
            Logger.Info("=== Test Complete ===");
             //Logger.Debug("--- Test Heartbeat ---");
        }
        

        void syncTest() {
            var t = new Thread(syncUpdate);
            t.Start();
        }
        void syncUpdate() {
            while (true) {
                for(int i = 0; i < clients.Length; ++i) {
                    var c = clients[i];
                    var cm = new CM_Sync();
                    cm.isOwner = (i == 0);
                    cm.syncData = "isOwner:" + cm.isOwner + " - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    c.dt = DateTime.Now;

                    cm.onResponse = (resp) => {
                        //var info = string.Format("C{0} Sync Response:{1}", c.pos, resp.ToString());
                        //Logger.Info(info);
                        //Logger.Info("delay:" + DateTime.Now.Subtract(c.dt).TotalMilliseconds + "ms");
                    };
                    c.send(cm);
                }
                Thread.Sleep(33);
            }
        }
       
    }
} 