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
    public partial class TCP_Server : ServerBase {

        
        private TcpListener listener;

        private Thread serverThread;
        private List<TcpClientConnection> activeConnections;
        private readonly object connectionsLock = new object();
        
        private int maxConnections;
        private int tcpConnectionTimeOut;


        public TCP_Server()
        {
            this.PortBase = Config.TCP_SERVER_PORT;

            //if (Instance == null) Instance = this;
            //else Logger.Error("TCP_Server instance already exists, using singleton pattern");

            this.maxConnections = Config.TCP_MAX_CONNECTIONS;
            this.tcpConnectionTimeOut = Config.TCP_CONNECTION_TIMEOUT;
            this._isRunning = false;
            this.activeConnections = new List<TcpClientConnection>();
            
            room = new Room();
            room.roomState = eRoomState.Idle;
            room.startTime = DateTime.Now;
            room.ID = Tools.IP2RoomID(Tools.GetLocalIP());
        }

        protected override void tryPort(){
            try{
                listener = new TcpListener(IPAddress.Any, _port);
                listener.Start();//block, may throw SocketException if port is taken
                _isRunning = true;

                serverThread = new Thread(ListenForClients);
                serverThread.IsBackground = true;
                serverThread.Start();
            }
            catch(Exception e) {
                throw;
            }
        }

        public void Stop()
        {
            if (!_isRunning)
                return;
            _isRunning = false;

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
            while (_isRunning)
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
                    if (_isRunning)
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
                while (connection.IsConnected && _isRunning)
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
            get { return _isRunning; }
        }

     
    }

} 