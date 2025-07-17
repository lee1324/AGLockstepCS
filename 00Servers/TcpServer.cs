using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;
using SimpleJson;
using System.Security;
using Servers;
using System.Diagnostics;

namespace AGSyncCS
{
    public class TcpServer {

        public void on(CM_Test cm, ref int errorCode, ref SM sm) {
            Logger.Instance.Info("server get cm_test:" + cm.ToString());
            var s = new SM_Test();
            s.i1 = 1111;
            s.str1 = "Hello from server.";
            sm = s;
        }
        public void on(CM_NewRoom cm, ref int errorCode, ref SM sm) {

        }

        public void on(CM_EnterRoom cm, ref int errorCode, ref SM sm) {

        }


        private TcpListener listener;
        private bool isRunning;
        private int port;
        private Thread serverThread;
        private List<TcpClientConnection> activeConnections;
        private readonly object connectionsLock = new object();
        private int maxConnections;
        private int connectionTimeout;

        public TcpServer(int port, int maxConnections = 100, int connectionTimeout = 30000)
        {
            this.port = port;
            this.maxConnections = maxConnections;
            this.connectionTimeout = connectionTimeout;
            this.isRunning = false;
            this.activeConnections = new List<TcpClientConnection>();
        }

        public void Start()
        {
            if (isRunning)
                return;

            try
            {
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                isRunning = true;

                serverThread = new Thread(ListenForClients);
                serverThread.IsBackground = true;
                serverThread.Start();

                Logger.Instance.Info(string.Format("TCP server:{0}", port));
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("Failed to start TCP server: " + ex.Message);
                throw;
            }
        }

        public void Stop()
        {
            if (!isRunning)
                return;

            isRunning = false;

            // Close all active connections
            lock (connectionsLock)
            {
                foreach (var connection in activeConnections)
                {
                    try
                    {
                        connection.Close();
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.Error("Error closing connection: " + ex.Message);
                    }
                }
                activeConnections.Clear();
            }

            // Stop the listener
            if (listener != null)
            {
                listener.Stop();
            }

            // Wait for server thread to finish
            if (serverThread != null && serverThread.IsAlive)
            {
                serverThread.Join(5000);
            }

            Logger.Instance.Info("TCP Server stopped");
        }

        private void ListenForClients()
        {
            while (isRunning)
            {
                try
                {
                    TcpClient client = listener.AcceptTcpClient();
                    
                    // Check if we've reached max connections
                    lock (connectionsLock)
                    {
                        if (activeConnections.Count >= maxConnections)
                        {
                            Logger.Instance.Warning("Maximum connections reached, rejecting new connection");
                            client.Close();
                            continue;
                        }
                    }

                    // Create new connection handler
                    TcpClientConnection connection = new TcpClientConnection(this, client, connectionTimeout);
                    
                    lock (connectionsLock)
                    {
                        activeConnections.Add(connection);
                    }

                    Logger.Instance.Info(string.Format("S new C:{0} (Total: {1})", 
                        client.Client.RemoteEndPoint, activeConnections.Count));

                    // Start connection handler in background thread
                    Thread connectionThread = new Thread(() => HandleConnection(connection));
                    connectionThread.IsBackground = true;
                    connectionThread.Start();
                }
                catch (Exception ex)
                {
                    if (isRunning)
                    {
                        Logger.Instance.Error("Error accepting TCP client: " + ex.Message);
                    }
                }
            }
        }

        private void HandleConnection(TcpClientConnection connection)//thread for each client
        {
            try
            {
                connection.Start();
                
                // Keep connection alive until it's closed
                while (connection.IsConnected && isRunning)
                {
                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("Error handling TCP connection: " + ex.Message);
            }
            finally
            {
                // Remove connection from active list
                lock (connectionsLock)
                {
                    activeConnections.Remove(connection);
                }
                
                Logger.Instance.Info(string.Format("TCP connection closed from {0} (Remaining: {1})", 
                    connection.RemoteEndPoint, activeConnections.Count));
            }
        }


        public int ActiveConnectionCount
        {
            get
            {
                lock (connectionsLock)
                {
                    return activeConnections.Count;
                }
            }
        }

        public bool IsRunning
        {
            get { return isRunning; }
        }

     
    }

    public class TcpClientConnection
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

        private void ReceiveLoop()//thread for each client
        {
            byte[] buffer = new byte[ServerConfig.BUFFER_SIZE];
            var ms = new MemoryStream(buffer);
            var reader = new BinaryReader(stream);
            while (isConnected)
            {
                try
                {
                    ms.Position = 0;//prepare for next, network stream doest support seek, use ms instead.
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);//block
                    if (bytesRead == 0) {
                        // Connection closed by client
                        break;
                    }
                    while(ms.Position < bytesRead) {//if client sends 2 cm in a for-loop.
                        int protocal = reader.ReadInt32();
                        Logger.Instance.Log(LogLevel.Debug, "S Protocal:" + protocal + " bytesRead:" + bytesRead);

                        CM cm = Protocals.GetCM(protocal);
                        SM sm = null;
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
                        if(protocal == Protocals.Test)
                            server.on((CM_Test)cm, ref errorCode, ref sm);
                        else if (protocal == Protocals.NewRoom)
                            server.on((CM_NewRoom)cm, ref errorCode, ref sm);
                        else if (protocal == Protocals.EnterRoom) 
                            server.on((CM_EnterRoom) cm , ref errorCode, ref sm);
                        Response(protocal, messasgeUID, errorCode, sm);//callback immediately
                    }
                }
                catch (Exception ex) {
                    if (isConnected) {
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
                byte[] buffer = new byte[ServerConfig.BUFFER_SIZE];
                var ms = new MemoryStream(buffer);
                var writer = new BinaryWriter(stream);
                ms.Seek(0, SeekOrigin.Begin);
                writer.Write((int)MessageType.Push);
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

            try
            {
                byte[] buffer = new byte[ServerConfig.BUFFER_SIZE];
                var ms = new MemoryStream(buffer);//tmp stream
                var writer = new BinaryWriter(ms);
                ms.Seek(0, SeekOrigin.Begin);

                writer.Write((int)MessageType.Response);
                writer.Write(protocal);
                writer.Write(messasgeUID);
                writer.Write(errorCode);//在push时无errorcode，但是push也用这套，省（个接口+推送判断）

                if (errorCode == ErrorCode.None)
                    sm.writeTo(writer);

                lock (streamLock) {
                    stream.Write(buffer, 0, (int)ms.Length);
                }
                Logger.Instance.Debug(string.Format("S 2C {0}: {1}", remoteEndPoint, sm.ToString()));
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