using AGSyncCS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AGSyncCS {
    partial class TcpClientConnection
    {
        private TcpClient client;
        private NetworkStream stream;
        private bool isConnected;
        private string remoteEndPoint;
        private int timeout;
        private Thread receiveThread;
        private readonly object streamLock = new object();
        private TcpServer server;

        public TcpClientConnection(TcpServer server, TcpClient client, int timeout)
        {
            this.server = server;
            this.client = client;
            this.timeout = timeout;
            this.isConnected = false;
            this.remoteEndPoint = client.Client.RemoteEndPoint.ToString();
        }

        public string RemoteEndPoint
        {
            get { return remoteEndPoint; }
        }

        public bool IsConnected
        {
            get { return isConnected; }
        }

        public void Start()//thread for each client
        {
            try
            {
                stream = client.GetStream();
                stream.ReadTimeout = timeout;
                stream.WriteTimeout = timeout;
                isConnected = true;

                // Start receive thread
                receiveThread = new Thread(ReceiveLoop);
                receiveThread.IsBackground = true;
                receiveThread.Start();

                Logger.Instance.Debug(string.Format("S TCP connection {0} started", remoteEndPoint));
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(string.Format("Error starting TCP connection {0}: {1}", remoteEndPoint, ex.Message));
                throw;
            }
        }

        private void ReceiveLoop()//thread for each client, no need to lock
        {
            byte[] buffer = new byte[Config.BUFFER_SIZE];
            var ms = new MemoryStream(buffer);
            var reader = new BinaryReader(ms);
            while (isConnected) {
                try {
                    ms.Position = 0;//prepare for next, network stream doest support seek, use ms instead.
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);//block
                    //Logger.Instance.Debug(string.Format("S Read from {0}, bytesRead: {1}", remoteEndPoint, bytesRead));
                    if (bytesRead == 0) {
                        Logger.Instance.Info("S Connection closed by client: " + remoteEndPoint);
                        break;
                    }

                    while (ms.Position < bytesRead) {//if client sends 2 cm in a for-loop.
                        int protocal = reader.ReadInt32();

                        CM cm = Protocals.GetCM(protocal);
                        int messasgeUID = 0;

                        if (cm == null) {//error or malicious
                            Logger.Instance.Warning($"CM == null, protocal: {protocal} from {remoteEndPoint}");
                            continue;
                        }
                        else {
                            messasgeUID = reader.ReadInt32();// protocal不存在uid也无必要, so Uid 放protocal之后
                            cm.readFrom(reader);
                        }

                        int errorCode = ErrorCode.None;//will be sent to client
                        SM response = null;
                        dispatch(protocal, cm, ref errorCode, ref response);//dispatch to specific on method
                        Response(protocal, messasgeUID, errorCode, response);//callback immediately(sync, async is not now supported)
                    }
                }
                catch (Exception ex) {
                    if (isConnected) {
                        //Dont READ or writer networkstream.Position, otherwise it will throw exception
                        Logger.Instance.Debug(string.Format("S Is C closed? Error receiving from {0}: {1}.", remoteEndPoint, ex.Message));
                    }
                    break;
                }
            }
        }

        public void Push(int protocal, SM sm) {
            if (!isConnected)
                throw new InvalidOperationException("Connection is not active");
            try
            {
                byte[] buffer = new byte[Config.BUFFER_SIZE];
                var ms = new MemoryStream(buffer);
                var writer = new BinaryWriter(stream);
                ms.Seek(0, SeekOrigin.Begin);
                writer.Write((int)eMessageType.Push);
                writer.Write(protocal);
                sm.writeTo(writer);
                lock (streamLock) {
                    stream.Write(buffer, 0, (int)ms.Length);
                }
                Logger.Instance.Debug(string.Format("Push to {0}: {1}", remoteEndPoint, sm.ToString()));
            }
            catch (Exception ex) {
                Logger.Instance.Error(string.Format("Error pushing to {0}: {1}", remoteEndPoint, ex.Message));
                throw;
            }
        }

        //前端不记录状态，加上后端还有push，所以protocal必须
        public void Response(int protocal, int messasgeUID, int errorCode, SM sm) {
            if (!isConnected)
                throw new InvalidOperationException("Connection is not active");

            try {
                lock (streamLock) {

                    byte[] buffer = new byte[Config.BUFFER_SIZE];
                    var ms = new MemoryStream(buffer);//tmp stream
                    var writer = new BinaryWriter(ms);
                    ms.Seek(0, SeekOrigin.Begin);

                    writer.Write((int)eMessageType.Response);
                    writer.Write(protocal);
                    writer.Write(messasgeUID);
                    writer.Write(errorCode);//在push时无errorcode，但是push也用这套，省（个接口+推送判断）

                    if (errorCode == ErrorCode.None){
                        if (sm == null) Logger.Instance.Warning("errorCode or sm, U forgot 2 set one of 'em!!!");
                        else {
                            sm.writeTo(writer);
                            stream.Write(buffer, 0, (int)ms.Length);
                            Logger.Instance.Debug(string.Format("S 2C {0}: {1}", remoteEndPoint, sm.ToString()));
                        }
                    }
                }
               
            }
            catch (Exception ex) {
                Logger.Instance.Error(string.Format("Error sending to {0}: {1}", remoteEndPoint, ex.Message));
                throw;
            }
        }

        public void Close() {
            isConnected = false;

            try
            {
                if (stream != null)
                {
                    stream.Close();
                }
                if (client != null)
                {
                    client.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(string.Format("Error closing TCP connection {0}: {1}", remoteEndPoint, ex.Message));
            }
        }
    }
}
