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
        private int port;

        private Thread serverThread;
        private List<TcpClientConnection> activeConnections;
        private readonly object connectionsLock = new object();
        
        private int maxConnections;
        private int connectionTimeout;

        public BandServer()
        {
            if (Instance == null) Instance = this;
            else Logger.Error("TCP_Server instance already exists, using singleton pattern");
            this.port = Config.TCP_SERVER_PORT;
            this.maxConnections = Config.TCP_MAX_CONNECTIONS;
            this.connectionTimeout = Config.TCP_CONNECTION_TIMEOUT;
            this.isRunning = false;
            this.activeConnections = new List<TcpClientConnection>();
            newRoom("my room"); // Create a local room
        }

        public void start() {
            //Step 00: owner starts server in local wifi.
            Thread tcpServerThread = new Thread(() => {
                _start();
                //Step 01: owner creates a local room
                // Keep TCP server running
                while (true) Thread.Sleep(1000);
            });
            tcpServerThread.IsBackground = true;
            tcpServerThread.Start();
        }

        public void _start()
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

                Logger.Info(string.Format("TCP server:{0}", port));
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to start TCP server: " + ex.Message);
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
                    TcpClientConnection connection = new TcpClientConnection(this, client, connectionTimeout);
                    
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