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

namespace AGSyncCS{
    public partial class CM {//extend cm for client's usage
        public Action<SM> onResponse = null;
        static TCPClientWrapper _Instance = null;
        //public static void InitConnection() {
        //    if (_Instance == null) {
        //        _Instance = new Client();
        //        Logger.Debug("C TcpClientWrapper InitConnection IP:" + Config.TCP_HOST
        //             + ":" + Config.TCP_SERVER_PORT);
        //        _Instance._connect(Config.TCP_HOST, Config.TCP_SERVER_PORT);
        //    }
        //}

        //public void send() {
        //    _Instance.Send(this);
        //}
    }
  
    class TCPClientWrapper
    {


        private System.Net.Sockets.TcpClient client;
        private NetworkStream stream;
        private bool isConnected;
        private string serverAddress;
        private int serverPort;
        private int timeout;
        private Thread receiveThread;
        private readonly object streamLock = new object();

        public TCPClientWrapper() {
            this.timeout = 30000;
        }

        Action _onSuccess;
        Action<string> _onFail;
        public TCPClientWrapper connect(string IP, int port, Action onSuccess, Action<string> onFail) {
            _onSuccess = onSuccess;
            _onFail    = onFail;
            serverPort = port;
            var t = new Thread(() => {
                _connect(IP, serverPort);
            });
            t.Start();
            return this;
        }

        void _connect(string serverAddress, int serverPort) {
            try {
                this.serverAddress = serverAddress;
                this.serverPort = serverPort;

                client = new System.Net.Sockets.TcpClient();
                client.Connect(serverAddress, serverPort);//NOTICE:THIS IS A BLOCK CALL !!!
                //Logger.Debug(string.Format("Connectting to TCP server {0}:{1}", serverAddress, serverPort));
                stream = client.GetStream();
                isConnected = true;

                // Start receive thread
                receiveThread = new Thread(ReceiveLoop);
                receiveThread.Start();

                var cm = new CM_TestServer();
                cm.shakeI = serverPort + 2;
                cm.onResponse = (sm_response) => {
                    var sm = (SM_TestServer)sm_response;
                    
                    if (sm.shakeI == cm.shakeI * 2) {
                        var heatBeatThread = new Thread(_heatBeatLoop);
                        heatBeatThread.Start();
                        if (_onSuccess != null) 
                            _onSuccess();
                    }
                    else { 
                        onShakeFail();
                        //1/2 要么能连，但不是我们的服务器（这里）
                        if (_onFail != null) _onFail("shake fail");
                    }
                };
                Send(cm);
            }
            catch (Exception ex) {
                //connect 有遍历重试不同port的机制，所以不要再抛了，在上面 成功/失败中处理
                Logger.Warning(string.Format("try but fail to connect to TCP server {0}:{1}:{2}",
                    serverAddress, serverPort, ex.Message));
                //If it's taken by another app, u wouldn't know
                //need a protocal of handshake
                //2/2 要么中途异常走这里 
                if (_onFail != null) _onFail(ex.Message);
            }
        }

        void onShakeFail() {
            Close();
        }

        byte[] sendBuffer = new byte[Config.BUFFER_SIZE];
        Dictionary<int, Action<SM>> _pushListeners = new Dictionary<int, Action<SM>>();//Protocal - Action
        Dictionary<int, Action<SM>> _listeners = new Dictionary<int, Action<SM>>();//MsgUID - Action

        public void onPush(int protocal, Action<SM> action) {
            _pushListeners[protocal] = action;
        }
        public void Send(CM cm)
        {
            Logger.Debug("C Send() " + cm.ToString());
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
                    if (protocal == Protocals.None) {
                        Logger.Error(string.Format("Unregistered protocal: {0}", cm.GetType()));
                        throw new InvalidOperationException("Unregistered protocal");
                    }
                    else
                    {
                        writer.Write(protocal);
                        writer.Write(GlobalUID);
                        cm.writeTo(writer);

                        _listeners[GlobalUID] = cm.onResponse;
                        stream.Write(sendBuffer, 0, (int)ms.Position);
                        stream.Flush();//加这行是否能立即发送？未确定（C连发两cm，后端会收到一个stream中，加不加这行都是)
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("C Error sending message: " + ex.Message);
                throw;
            }
        }

        void _heatBeatLoop() {
            while(isConnected) {
                try {
                    var cm = new CM_HeartBeat();//heratBeat is more like a INNER MESSAGE
                    cm.lastBeatTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    cm.onResponse = (sm_response) => {
                        var sm = (SM_HeartBeat)sm_response;
                        //lstodo heartbeat
                    };

                    this.Send(cm);
                    Thread.Sleep(Config.HEARTBEAT_INTERVAL);
                }
                catch (Exception ex) {
                    Logger.Error("C Error in heartbeat loop: " + ex.Message);
                }
            }
        }

        static int GlobalUID = 0;
        //后端主动推的push消息，根据protocal设置监听
        //前端发的（同一protocol会发很多个），据msguid设置监听
        private void ReceiveLoop()
        {
            byte[] buffer = new byte[Config.BUFFER_SIZE];
            var ms = new MemoryStream(buffer);
            var reader = new BinaryReader(ms);
            while (isConnected) {
                try {
                    //bytesRead = 4096(max size), why?
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);//block且一次只读一个sm（不存在并发，所以下面不锁

                    if (bytesRead == 0) { // Connection closed by server
                        Logger.Info("Server call Close() or ShutDown()");
                        break;
                    }
                    ms.Seek(0, SeekOrigin.Begin);
                    var iMessageType = reader.ReadInt32();
                    var protocal = reader.ReadInt32();
                    //Logger.Debug("C iMessageType:" + iMessageType + " protocal:" + protocal);

                    SM sm = null;
                    if (iMessageType == (int)eMessageType.Push) {
                         sm = Protocals.GetSM(protocal);
                         if(sm == null)
                             Logger.Warning("SM not found by protocal:" + protocal);
                         else {
                             sm.readFrom(reader);
                             Action<SM> ls;
                             if(_pushListeners.TryGetValue(protocal, out ls)) 
                                ls(sm);
                         }
                    }
                    else if (iMessageType == (int)eMessageType.Response)
                    {
                        var msgUID = reader.ReadInt32();
                        var errorCode = reader.ReadInt32();

                        Logger.Log(LogLevel.Debug, string.Format("C protocal:{0} " +
                            "messageUID:{1} errorCode:{2}",
                            protocal, msgUID, errorCode));

                        if (errorCode == ErrorCode.None) {
                            sm = Protocals.GetSM(protocal);
                            if(sm == null)
                                Logger.Warning("SM not found by protocal:" + protocal);
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
                            Logger.Warning(string.Format("lstodo cm errorcode {0}", errorCode));
                        }
                    }
                    else Logger.Info("Wrong MessageType From Server:" + iMessageType);
                }
                catch (Exception ex) {
                    if (isConnected){
                        Logger.Debug("Perhaps be a retry port, debug receiving from server: " + ex.Message);
                        
                    }
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

                Logger.Info("TCP client connection closed");
            }
            catch (Exception ex)
            {
                Logger.Error("Error closing TCP client: " + ex.Message);
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