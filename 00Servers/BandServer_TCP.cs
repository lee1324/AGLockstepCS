using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;
using SimpleJson;
using System.Security;
using System.Diagnostics;

namespace AGSyncCS
{
    public partial class BandServer {

        public static BandServer Instance;
        
        private TcpListener listener;
        private bool isRunning;
        private int tcpPort;

        private Thread serverThread;
        private List<TcpClientConnection> activeConnections;
        private readonly object connectionsLock = new object();
        
        private int maxConnections;
        private int tcpConnectionTimeOut;

        private UdpServer _udpServer;

        public BandServer()
        {
            if (Instance == null) Instance = this;
            else Logger.Error("TCP_Server instance already exists, using singleton pattern");
            this.tcpPort = Config.TCP_SERVER_PORT;
            this.maxConnections = Config.TCP_MAX_CONNECTIONS;
            this.tcpConnectionTimeOut = Config.TCP_CONNECTION_TIMEOUT;
            this.isRunning = false;
            this.activeConnections = new List<TcpClientConnection>();
            
            room = new Room();
            room.roomState = eRoomState.Idle;
            room.startTime = DateTime.Now;
            room.ID = Tools.IP2RoomID(Tools.GetLocalIP());
        }

 

        public void start(Action onSuccess, Action<string> onFail)
        {
            if (isRunning)
                return;

            int retryTimes = 0;
            while(!isRunning){
                try {
                    if(retryTimes > 0) {
                        Logger.Warning("retry times:" + retryTimes);
                    }
                    tryTCPListenerOnPort();
                    if (onSuccess != null) onSuccess();
                }
                catch (Exception ex) {//Port taken
                   
                    Logger.Warning("Failed to start TCP server: " + ex.Message);

                    if(retryTimes >= Config.MAX_PORT_RETRY) {
                        if(onFail != null) onFail(ex.Message);
                        break;
                    }
                    else {
                        ++retryTimes;
                        tcpPort = Config.TCP_SERVER_PORT + retryTimes;//retry on different port
                    }
                }
            }


            try {
                _udpServer = new UdpServer(Config.UDP_SERVER_PORT, Config.UDP_ECHO_ENABLED);
                _udpServer.start();
            }
            catch (Exception ex) {
                //Port taken
                Logger.Error("Failed to start TCP server: " + ex.Message);
                throw;
            }
        }

        void tryTCPListenerOnPort() {
            listener = new TcpListener(IPAddress.Any, tcpPort);
            listener.Start();
            isRunning = true;

            serverThread = new Thread(ListenForClients);
            serverThread.IsBackground = true;
            serverThread.Start();
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
                        Logger.Error("Error closing connection: " + ex.Message);
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

            Logger.Info("TCP Server stopped");
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
                            Logger.Warning("Maximum connections reached, rejecting new connection");
                            client.Close();
                            continue;
                        }
                    }

                    // Create new connection handler
                    TcpClientConnection connection = new TcpClientConnection(this, client, tcpConnectionTimeOut);
                    
                    lock (connectionsLock)
                    {
                        activeConnections.Add(connection);
                    }

                    //Logger.Debug(string.Format("S new C:{0} (Total: {1})", 
                    //    client.Client.RemoteEndPoint, activeConnections.Count));

                    // Start connection handler in background thread
                    Thread connectionThread = new Thread(() => HandleConnection(connection));
                    connectionThread.IsBackground = true;
                    connectionThread.Start();
                }
                catch (Exception ex)
                {
                    if (isRunning)
                    {
                        Logger.Error("Error accepting TCP client: " + ex.Message);
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
                Logger.Error("Error handling TCP connection: " + ex.Message);
            }
            finally
            {
                // Remove connection from active list
                lock (connectionsLock)
                {
                    activeConnections.Remove(connection);
                }
                
                Logger.Info(string.Format("TCP connection closed from {0} (Remaining: {1})", 
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

} 