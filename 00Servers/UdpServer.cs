using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;

namespace AGSyncCS
{
    public partial class UdpServer
    {
        private int port;
        private UdpClient udpClient;
        private Thread listenThread;
        private bool isRunning;
        private bool echoBack;

        public UdpServer(int port, bool echoBack = false)
        {
            this.port = port;
            this.echoBack = echoBack;
            this.isRunning = false;
        }

         public void start()
         {
            if (isRunning)
                return;

            udpClient = new UdpClient(port);
            isRunning = true;
            listenThread = new Thread(ListenLoop);
            listenThread.IsBackground = true;
            listenThread.Start();
            Logger.Info("UDP server started on port " + port);
        }

        public void Stop()
        {
            isRunning = false;
            if (udpClient != null)
            {
                udpClient.Close();
            }
            if (listenThread != null && listenThread.IsAlive)
            {
                listenThread.Join(1000);
            }
            Logger.Info("UDP server stopped.");
        }

        private void ListenLoop()
        {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(Config.ANY_ADDRESS), 0);
            while (isRunning)
            {
                try
                {
                    byte[] data = udpClient.Receive(ref remoteEP);
                    var reader = new BinaryReader(new MemoryStream(data));
                    
                    int protocal = reader.ReadInt32();
                    int messageUID = reader.ReadInt32();

                    var cm = Protocals.GetCM(protocal);
                    if(cm == null) {
                        Logger.Warning("CM not found by protocal:" + protocal);
                        continue;
                    }
                    cm.readFrom(reader);

                    int errorCode = ErrorCode.None;
                    SM sm = null;
                    if(cm.GetType() == typeof(CM_Sync)) {//dispatch()
                        on((CM_Sync)cm, ref errorCode, ref sm);
                    }

                    var sendBuffer = new byte[Config.BUFFER_SIZE];
                    var ms = new MemoryStream(sendBuffer);
                    var writer = new BinaryWriter(ms);
                    writer.Write(protocal);
                    writer.Write(messageUID);
                    writer.Write(errorCode);
                    
                    if (errorCode == ErrorCode.None)
                    {
                        if (sm == null) Logger.Warning("errorCode or sm, U forgot 2 set one of 'em!!!");
                        else{
                            sm.writeTo(writer);
                            udpClient.Send(sendBuffer, (int)ms.Position, remoteEP);
                        }
                    }
                }
                catch (SocketException)
                {
                    // Socket closed, exit loop
                    break;
                }
                catch (Exception ex)
                {
                    Logger.Error("UDP server error: " + ex.Message);
                }
            }
        }
    }
} 