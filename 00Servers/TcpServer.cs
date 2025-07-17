using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;
using SimpleJson;
using System.Security;

namespace AGSyncCS
{
    public class TcpServer {
        public void on(CM_NewRoom cm, ref int errorCode, ref SM_NewRoom sm)
        {

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

                LogService.Instance.Info(string.Format("TCP Server started on port {0}", port));
            }
            catch (Exception ex)
            {
                LogService.Instance.Error("Failed to start TCP server: " + ex.Message);
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
                        LogService.Instance.Error("Error closing connection: " + ex.Message);
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

            LogService.Instance.Info("TCP Server stopped");
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
                            LogService.Instance.Warning("Maximum connections reached, rejecting new connection");
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

                    LogService.Instance.Info(string.Format("New TCP connection from {0} (Total: {1})", 
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
                        LogService.Instance.Error("Error accepting TCP client: " + ex.Message);
                    }
                }
            }
        }

        private void HandleConnection(TcpClientConnection connection)
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
                LogService.Instance.Error("Error handling TCP connection: " + ex.Message);
            }
            finally
            {
                // Remove connection from active list
                lock (connectionsLock)
                {
                    activeConnections.Remove(connection);
                }
                
                LogService.Instance.Info(string.Format("TCP connection closed from {0} (Remaining: {1})", 
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

        public void Start()
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

                LogService.Instance.Debug(string.Format("TCP connection {0} started", remoteEndPoint));
            }
            catch (Exception ex)
            {
                LogService.Instance.Error(string.Format("Error starting TCP connection {0}: {1}", remoteEndPoint, ex.Message));
                throw;
            }
        }

        private void ReceiveLoop()
        {
            byte[] buffer = new byte[ServerConfig.BUFFER_SIZE];
            var ms = new MemoryStream(buffer);
            var reader = new BinaryReader(ms);
            while (isConnected)
            {
                try
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        // Connection closed by client
                        break;
                    }

                    ms.Position = 0;
                    int protocal = reader.ReadInt32();
                    int errorCode = ErrorCode.None;//will be sent to client
                    int UID = 0;//Uid 放protocal之后（因为 protocal不存在uid也无必要）

                    if (protocal == Protocals.NewRoom)
                    {
                        UID = reader.ReadInt32();
                        var cm = new CM_NewRoom();
                        cm.readFrom(reader);

                        SM_NewRoom sm = null;
                        server.on(cm, ref errorCode, ref sm);
                        Send(protocal, errorCode, sm);
                    }
                    else if (protocal == Protocals.EnterRoom)
                    {
                        var cm = new CM_EnterRoom();
                        SM_EnterRoom sm = null;

                        //lstodo
                    }
                    else
                    {
                        // Unknown protocol/malicious client, send error response
                        // var sm = new SM();
                        // sm.errorCode = ErrorCode.Unknown_Protocal; // or a suitable error code
                        // Send(sm);
                        //lstodo close client
                        LogService.Instance.Warning($"Unknown protocol: {protocal} from {remoteEndPoint}");
                    }

                }
                catch (Exception ex)
                {
                    if (isConnected)
                    {
                        LogService.Instance.Error(string.Format("Error receiving from {0}: {1}", remoteEndPoint, ex.Message));
                    }
                    break;
                }
            }
        }

        
        //前端不记录状态，加上后端还有push，所以protocal必须
        public void Send(int protocal, int errorCode, SM sm)
        {
            if (!isConnected)
                throw new InvalidOperationException("Connection is not active");

            try
            {
                byte[] buffer = new byte[ServerConfig.BUFFER_SIZE];
                var ms = new MemoryStream(buffer);
                var writer = new BinaryWriter(stream);
                ms.Seek(0, SeekOrigin.Begin);

                writer.Write((int)MessageType.Response);
                writer.Write(protocal);

                writer.Write(errorCode);//在push时无errorcode，但是push也用这套，省（个接口+推送判断）
                if (errorCode == ErrorCode.None)
                    sm.writeTo(writer);

                lock (streamLock)
                {
                    stream.Write(buffer, 0, (int)ms.Length);
                }

                LogService.Instance.Debug(string.Format("Sent to {0}: {1}", remoteEndPoint, sm.ToString()));
            }
            catch (Exception ex)
            {
                LogService.Instance.Error(string.Format("Error sending to {0}: {1}", remoteEndPoint, ex.Message));
                throw;
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
            }
            catch (Exception ex)
            {
                LogService.Instance.Error(string.Format("Error closing TCP connection {0}: {1}", remoteEndPoint, ex.Message));
            }
        }
    }
} 