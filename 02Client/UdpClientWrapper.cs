using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AGSyncCS
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
            Logger.Info(string.Format("Connected to UDP server at {0}:{1}", serverIP, port));
        }

        public void Connect(IPAddress serverIP, int port)
        {
            serverEndPoint = new IPEndPoint(serverIP, port);
            isConnected = true;
            Logger.Info(string.Format("Connected to UDP server at {0}:{1}", serverIP, port));
        }


        byte[] _sendBuffer = null;
        MemoryStream _sendMS = null;
        BinaryWriter _writer = null;

        static int GlobalUID = 0;
        Dictionary<int, Action<SM>> _listeners = null;

        void _send(CM cm, int timeoutMs = 5000)
        {
            if (!isConnected)
            {
                throw new InvalidOperationException("Not connected to server");
            }

            // Send the message
             if(_sendBuffer == null) {
                _sendBuffer = new byte[Config.BUFFER_SIZE];
                _sendMS = new MemoryStream(_sendBuffer);
                _writer = new BinaryWriter(_sendMS);

                _listeners = new Dictionary<int, Action<SM>>();
            }

             if(true){
                _sendMS.Seek(0, SeekOrigin.Begin);
                int protocal = Protocals.Sync;

                _writer.Write(protocal);
                _writer.Write(GlobalUID++);
                cm.writeTo(_writer);
            }

            _listeners[GlobalUID] = cm.onResponse;
            udpClient.Send(_sendBuffer, (int)_sendMS.Position, serverEndPoint);

            // Wait for response
            try
            {
                udpClient.Client.ReceiveTimeout = timeoutMs;
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(Config.ANY_ADDRESS), 0);

                byte[] responseData = udpClient.Receive(ref remoteEP);

                BinaryReader reader = new BinaryReader(new MemoryStream(responseData));
                int protocal   = reader.ReadInt32();
                int messageUID = reader.ReadInt32();
                var sm = Protocals.GetSM(protocal);

                if (sm == null){
                    Logger.Error("Received unknown protocol: " + protocal);
                    return;
                }
                sm.readFrom(reader);
                if (_listeners.ContainsKey(messageUID)) {
                    var ls = _listeners[messageUID];
                    ls(sm);
                    _listeners.Remove(messageUID);
                }
            }
            catch (SocketException ex) {
                if (ex.SocketErrorCode == SocketError.TimedOut) {
                    Logger.Warning("Timeout waiting for response");
                }
                throw;
            }
        }

        public void Send(CM cm)
        {
            if (!isConnected) {
                throw new InvalidOperationException("Not connected to server");
            }

            Thread thread = new Thread(() => {
                try {
                    _send(cm);
                }
                catch (Exception ex) {
                    Logger.Error("Async send error: " + ex.Message);
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
                Logger.Info("UDP client closed");
            }
        }

        public bool IsConnected
        {
            get { return isConnected; }
        }
    }
} 