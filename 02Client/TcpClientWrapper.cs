using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using SimpleJson;
using System.IO;

namespace AGSyncCS
{

    public partial class CM//extend cm for client's usage
    {
        Action<SM> _onResponse = null;
        /// <summary>
        /// 有了errorCode就不会调用
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public CM onResponse(Action<SM> action)
        {
            _onResponse = action;
            return this;
        }

        public void send()
        {
            TcpClientWrapper.Instance.Send(this);
        }
    }
  
    internal class TcpClientWrapper
    {
        private static TcpClientWrapper _Instance = null;
        /// <summary>
        /// 组队服，长连
        /// </summary>
        public static TcpClientWrapper Instance
        {
            get
            {
                if (_Instance == null)
                {
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
        private Action<string> messageReceivedCallback;
        private ConcurrentQueue<string> receivedMessages;
        private AutoResetEvent messageReceivedEvent;

        public TcpClientWrapper(int timeout = 30000)
        {
            this.timeout = timeout;
            this.receivedMessages = new ConcurrentQueue<string>();
            this.messageReceivedEvent = new AutoResetEvent(false);
        }

        public void Connect(string serverAddress, int serverPort)
        {
            try
            {
                this.serverAddress = serverAddress;
                this.serverPort = serverPort;

                client = new System.Net.Sockets.TcpClient();
                client.Connect(serverAddress, serverPort);
                stream = client.GetStream();
                isConnected = true;

                LogService.Instance.Info(string.Format("Connected to TCP server {0}:{1}", serverAddress, serverPort));

                // Start receive thread
                receiveThread = new Thread(ReceiveLoop);
                receiveThread.IsBackground = true;
                receiveThread.Start();
            }
            catch (Exception ex)
            {
                LogService.Instance.Error(string.Format("Failed to connect to TCP server {0}:{1}: {2}",
                    serverAddress, serverPort, ex.Message));
                throw;
            }
        }

        public void Connect(IPAddress serverAddress, int serverPort)
        {
            Connect(serverAddress.ToString(), serverPort);
        }

        public void SetMessageReceivedCallback(Action<string> callback)
        {
            this.messageReceivedCallback = callback;
        }

        byte[] sendBuffer = new byte[ServerConfig.BUFFER_SIZE];
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
                    var writer = new BinaryWriter(stream);
                    cm.writeTo(writer);


                    stream.Write(sendBuffer, 0, (int)ms.Length);
                }

                LogService.Instance.Debug(string.Format("Sent to server: {0}", message));
            }
            catch (Exception ex)
            {
                LogService.Instance.Error("Error sending message: " + ex.Message);
                throw;
            }
        }

        public void Send(TCPRequest request)
        {

        }

        static int GlobalUID = 0;
        public string SendAndReceive(string path, JsonNode js, int receiveTimeout = 5000)
        {
            ++GlobalUID;
            if (!isConnected)
                throw new InvalidOperationException("Not connected to server");
            try
            {
                // Clear any previous messages
                string previousMessage;
                while (receivedMessages.TryDequeue(out previousMessage)) { }
                // Send the message
                Send(path, js);
                // Wait for response with timeout
                if (messageReceivedEvent.WaitOne(receiveTimeout))
                {
                    if (receivedMessages.TryDequeue(out string response))
                    {
                        LogService.Instance.Info(string.Format("Received response: {0}", response));
                        return response;
                    }
                }
                LogService.Instance.Warning("No response received from server within timeout");
                return null;
            }
            catch (Exception ex)
            {
                LogService.Instance.Error("Error in send and receive: " + ex.Message);
                throw;
            }
        }
        //后端主动推的push消息，根据protocal设置监听
        //前端发的（同一protocol会发很多个），据msguid设置监听
        private void ReceiveLoop()
        {
            byte[] buffer = new byte[ServerConfig.BUFFER_SIZE];

            while (isConnected)
            {
                try
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        // Connection closed by server
                        LogService.Instance.Info("Server closed the connection");
                        break;
                    }
                    var ms = new MemoryStream(buffer);
                    var reader = new BinaryReader(ms);

                    var tp = reader.ReadInt32();
                    var protocal = reader.ReadInt32();

                    if (tp == (int)MessageType.Push) ;//lstodo push message
                    else if (tp == (int)MessageType.Response)
                    {
                        var msgUID = reader.ReadInt32();
                        var errorCode = reader.ReadInt32();

                        CM cm = null;
                        if (errorCode == ErrorCode.None)
                        {
                            if (protocal == Protocals.NewRoom) cm = new CM_NewRoom();
                            else if (protocal == Protocals.EnterRoom) cm = new CM_EnterRoom();
                            else LogService.Instance.Info(string.Format("unregistered protocal: {0}", protocal));//ls todo

                            cm.readFrom(reader);
                        }
                        else
                        {//lstodo errorCode
                            LogService.Instance.Info(string.Format("lstodo cm errorcode {0}", errorCode));
                        }
                    }
                    else
                    {
                        LogService.Instance.Info("Wrong MessageType From Server:" + tp);
                    }

                }
                catch (Exception ex)
                {
                    if (isConnected)
                    {
                        LogService.Instance.Error("Error receiving from server: " + ex.Message);
                    }
                    break;
                }
            }
        }

        public void SendAsync(string path, JsonNode js, Action<string> callback = null)
        {
            if (!isConnected)
                throw new InvalidOperationException("Not connected to server");
            Thread thread = new Thread(() =>
            {
                try
                {
                    string response = SendAndReceive(path, js);
                    if (callback != null)
                    {
                        callback(response);
                    }
                }
                catch (Exception ex)
                {
                    LogService.Instance.Error("Async send error: " + ex.Message);
                    if (callback != null)
                    {
                        callback(null);
                    }
                }
            });
            thread.IsBackground = true;
            thread.Start();
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

                LogService.Instance.Info("TCP client connection closed");
            }
            catch (Exception ex)
            {
                LogService.Instance.Error("Error closing TCP client: " + ex.Message);
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