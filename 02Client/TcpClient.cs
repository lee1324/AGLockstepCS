using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using SimpleJson;

namespace AGServer
{
    public class TcpClientWrapper
    {
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

        public void Send(string path, JsonNode js)
        {
            string message = js.ToString();
            if (!isConnected)
                throw new InvalidOperationException("Not connected to server");

            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                
                lock (streamLock)
                {
                    stream.Write(data, 0, data.Length);
                }
                
                LogService.Instance.Debug(string.Format("Sent to server: {0}", message));
            }
            catch (Exception ex)
            {
                LogService.Instance.Error("Error sending message: " + ex.Message);
                throw;
            }
        }

        public string SendAndReceive(string path, JsonNode js, int receiveTimeout = 5000) {
            if (!isConnected)
                throw new InvalidOperationException("Not connected to server");
            try {
                // Clear any previous messages
                string previousMessage;
                while (receivedMessages.TryDequeue(out previousMessage)) { }
                // Send the message
                Send(path, js);
                // Wait for response with timeout
                if (messageReceivedEvent.WaitOne(receiveTimeout)) {
                    if (receivedMessages.TryDequeue(out string response)) {
                        LogService.Instance.Info(string.Format("Received response: {0}", response));
                        return response;
                    }
                }
                LogService.Instance.Warning("No response received from server within timeout");
                return null;
            } catch (Exception ex) {
                LogService.Instance.Error("Error in send and receive: " + ex.Message);
                throw;
            }
        }

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

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    LogService.Instance.Info(string.Format("Received from server: {0}", message));

                    // Add to queue for synchronous reads
                    receivedMessages.Enqueue(message);
                    messageReceivedEvent.Set();

                    // Invoke callback if set
                    if (messageReceivedCallback != null)
                    {
                        try
                        {
                            messageReceivedCallback(message);
                        }
                        catch (Exception ex)
                        {
                            LogService.Instance.Error("Error in message callback: " + ex.Message);
                        }
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

        public void SendAsync(string path, JsonNode js, Action<string> callback = null) {
            if (!isConnected)
                throw new InvalidOperationException("Not connected to server");
            Thread thread = new Thread(() => {
                try {
                    string response = SendAndReceive(path, js);
                    if (callback != null) {
                        callback(response);
                    }
                } catch (Exception ex) {
                    LogService.Instance.Error("Async send error: " + ex.Message);
                    if (callback != null) {
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