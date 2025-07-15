using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AGServer
{
    public class UdpClientWrapper
    {
        private System.Net.Sockets.UdpClient udpClient;
        private IPEndPoint serverEndPoint;
        private bool isConnected;

        public UdpClientWrapper()
        {
            udpClient = new System.Net.Sockets.UdpClient();
            isConnected = false;
        }

        public void Connect(string serverIP, int port)
        {
            serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIP), port);
            isConnected = true;
            LogService.Instance.Info(string.Format("Connected to UDP server at {0}:{1}", serverIP, port));
        }

        public void Connect(IPAddress serverIP, int port)
        {
            serverEndPoint = new IPEndPoint(serverIP, port);
            isConnected = true;
            LogService.Instance.Info(string.Format("Connected to UDP server at {0}:{1}", serverIP, port));
        }

        public void Send(string message)
        {
            if (!isConnected)
            {
                throw new InvalidOperationException("Not connected to server");
            }

            byte[] data = Encoding.UTF8.GetBytes(message);
            udpClient.Send(data, data.Length, serverEndPoint);
            LogService.Instance.Info("Sent: " + message);
        }

        public string SendAndReceive(string message, int timeoutMs = 5000)
        {
            if (!isConnected)
            {
                throw new InvalidOperationException("Not connected to server");
            }

            // Send the message
            Send(message);

            // Wait for response
            try
            {
                udpClient.Client.ReceiveTimeout = timeoutMs;
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(ServerConfig.ANY_ADDRESS), 0);
                byte[] responseData = udpClient.Receive(ref remoteEP);
                string response = Encoding.UTF8.GetString(responseData);
                LogService.Instance.Info(string.Format("Received from {0}: {1}", remoteEP, response));
                return response;
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.TimedOut)
                {
                    LogService.Instance.Warning("Timeout waiting for response");
                    return null;
                }
                throw;
            }
        }

        public void SendAsync(string message, Action<string> callback = null)
        {
            if (!isConnected)
            {
                throw new InvalidOperationException("Not connected to server");
            }

            Thread thread = new Thread(() =>
            {
                try
                {
                    string response = SendAndReceive(message);
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
            if (udpClient != null)
            {
                udpClient.Close();
                isConnected = false;
                LogService.Instance.Info("UDP client closed");
            }
        }

        public bool IsConnected
        {
            get { return isConnected; }
        }
    }
} 