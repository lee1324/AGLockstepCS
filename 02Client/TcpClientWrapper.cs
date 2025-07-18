using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using SimpleJson;
using System.IO;
using System.Collections.Generic;
using System.Net.Configuration;

namespace AGSyncCS
{

    public partial class CM//extend cm for client's usage
    {
        public Action<SM> onResponse = null;

        public void send() {
            TcpClientWrapper.Instance.Send(this);
        }
    }
  
    internal class TcpClientWrapper
    {
        private static TcpClientWrapper _Instance = null;
        /// <summary>
        /// 组队服，长连
        /// </summary>
        public static TcpClientWrapper Instance {
            get {
                if (_Instance == null) {
                    _Instance = new TcpClientWrapper();
                    _Instance.Connect(ServerConfig.TCP_SERVER_ADDRESS, ServerConfig.TCP_SERVER_PORT);
                }
                return _Instance;
            }
        }


        private System.Net.Sockets.TcpClient client;
        private NetworkStream stream;
        private bool isConnected;
        private string serverAddress;
        private int serverPort;
        private int timeout;
        private Thread receiveThread;
        private readonly object streamLock = new object();

        public TcpClientWrapper(int timeout = 30000)
        {
            this.timeout = timeout;
        }

        public void Connect(string serverAddress, int serverPort) {
            try {
                this.serverAddress = serverAddress;
                this.serverPort = serverPort;

                client = new System.Net.Sockets.TcpClient();
                client.Connect(serverAddress, serverPort);
                stream = client.GetStream();
                isConnected = true;

                // Start receive thread
                receiveThread = new Thread(ReceiveLoop);
                receiveThread.IsBackground = true;
                receiveThread.Start();
            }
            catch (Exception ex) {
                Logger.Instance.Error(string.Format("Failed to connect to TCP server {0}:{1}: {2}",
                    serverAddress, serverPort, ex.Message));
                throw;
            }
        }

        byte[] sendBuffer = new byte[ServerConfig.BUFFER_SIZE];
        Dictionary<int, Action<SM>> _listeners = new Dictionary<int, Action<SM>>();
        public void Send(CM cm)
        {
            ++GlobalUID;
            if (!isConnected)
                throw new InvalidOperationException("Not connected to server");
            try
            {
                lock (streamLock)
                {
                    var ms = new MemoryStream(sendBuffer);
                    var writer = new BinaryWriter(ms);

                    int protocal = Protocals.GetProtocal(cm); 
                    if (protocal == Protocals.None)
                    {
                        Logger.Instance.Error(string.Format("Unregistered protocal: {0}", cm.GetType()));
                        throw new InvalidOperationException("Unregistered protocal");
                    }
                    else
                    {
                        writer.Write(protocal);
                        writer.Write(GlobalUID);
                        cm.writeTo(writer);

                        _listeners[GlobalUID] = cm.onResponse;
                        Logger.Instance.Debug("C netStream Write once, ms.Position:" + ms.Position);
                        stream.Write(sendBuffer, 0, (int)ms.Position);

                        stream.Flush();//加这行是否能立即发送？未确定（C连发两cm，后端会收到一个stream中，加不加这行都是)
                    }
                }

                Logger.Instance.Debug(string.Format("C Send CM:{0}", cm.ToString()));
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("C Error sending message: " + ex.Message);
                throw;
            }
        }

        static int GlobalUID = 0;
        //后端主动推的push消息，根据protocal设置监听
        //前端发的（同一protocol会发很多个），据msguid设置监听
        private void ReceiveLoop()
        {
            byte[] buffer = new byte[ServerConfig.BUFFER_SIZE];
            while (isConnected) {
                try {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);//block且一次只读一个sm（不存在并发，所以下面不锁

                    if (bytesRead == 0) { // Connection closed by server
                        Logger.Instance.Info("Server closed the connection");
                        break;
                    }
                    var reader = new BinaryReader(stream);
                    var iMessageType = reader.ReadInt32();
                    var protocal = reader.ReadInt32();
                    //Logger.Instance.Log(LogLevel.Debug, "C iMessageType:" + iMessageType + " protocal:" + protocal);

                    if (iMessageType == (int)MessageType.Push) ;//lstodo
                    else if (iMessageType == (int)MessageType.Response)
                    {
                        var msgUID = reader.ReadInt32();
                        var errorCode = reader.ReadInt32();

                        Logger.Instance.Log(LogLevel.Debug, string.Format("C protocal:{0} " +
                            "msgUID:{1} errorCode:{2}",
                            protocal, msgUID, errorCode));

                        if (errorCode == ErrorCode.None) {
                            var sm = Protocals.GetSM(protocal);
                            if(sm == null)
                                Logger.Instance.Warning("SM not found by protocal:" + protocal);
                            else {
                                sm.readFrom(reader);
                                lock (_listeners) {//收发并发，锁
                                    if (_listeners.ContainsKey(msgUID)) {//Response类消息处理完就移除监听
                                        var ls = _listeners[msgUID];
                                        ls(sm);
                                        _listeners.Remove(msgUID);
                                    }
                                }
                            }
                        }
                        else {//lstodo errorCode
                            Logger.Instance.Info(string.Format("lstodo cm errorcode {0}", errorCode));
                        }
                    }
                    else Logger.Instance.Info("Wrong MessageType From Server:" + iMessageType);
                }
                catch (Exception ex) {
                    if (isConnected)
                        Logger.Instance.Error("Error receiving from server: " + ex.Message);
                    break;
                }
            }
        }

        public void Close()
        {
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

                Logger.Instance.Info("TCP client connection closed");
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("Error closing TCP client: " + ex.Message);
            }
        }

        public bool IsConnected
        {
            get { return isConnected; }
        }

        public string ServerAddress
        {
            get { return serverAddress; }
        }

        public int ServerPort
        {
            get { return serverPort; }
        }
    }
} 