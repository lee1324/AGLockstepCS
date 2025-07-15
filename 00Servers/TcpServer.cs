using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;

namespace AGServer
{
    public class TcpServer
    {
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
                    TcpClientConnection connection = new TcpClientConnection(client, connectionTimeout);
                    
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

        public void Broadcast(string message)
        {
            lock (connectionsLock)
            {
                foreach (var connection in activeConnections)
                {
                    try
                    {
                        connection.Send(message);
                    }
                    catch (Exception ex)
                    {
                        LogService.Instance.Error("Error broadcasting to connection: " + ex.Message);
                    }
                }
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

        public TcpClientConnection(TcpClient client, int timeout)
        {
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
                    string path = reader.ReadString();
                    string js = reader.ReadString();

                    string message = path + js;
                    LogService.Instance.Info(string.Format("Received from {0}: {1}", remoteEndPoint, message));

                    // Echo the message back
                    Send("Echo: " + message);
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

        public void Send(string message)
        {
            if (!isConnected)
                throw new InvalidOperationException("Connection is not active");

            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                
                lock (streamLock)
                {
                    stream.Write(data, 0, data.Length);
                }
                
                LogService.Instance.Debug(string.Format("Sent to {0}: {1}", remoteEndPoint, message));
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