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

        public UdpClientWrapper(string serverIP, int port)
        {
            udpClient = new System.Net.Sockets.UdpClient();
            serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIP), port);
        }

        byte[] _sendBuffer = null;
        MemoryStream _sendMS = null;
        BinaryWriter _writer = null;

        static int GlobalUID = 0;
        Dictionary<int, Action<SM>> _listeners = null;

        object _lock = new object();

        void _send(CM cm, int timeoutMs = 100)
        {
            ++GlobalUID;

            // Send the message
             if(_sendBuffer == null) {
                _sendBuffer = new byte[Config.BUFFER_SIZE];
                _sendMS = new MemoryStream(_sendBuffer);
                _writer = new BinaryWriter(_sendMS);

                _listeners = new Dictionary<int, Action<SM>>();
            }

            _sendMS.Seek(0, SeekOrigin.Begin);
            int protocal = Protocals.GetProtocal(cm);

            _writer.Write(protocal);
            _writer.Write(GlobalUID);
            cm.writeTo(_writer);

            _listeners[GlobalUID] = cm.onResponse;
            udpClient.Send(_sendBuffer, (int)_sendMS.Position, serverEndPoint);

            // Wait for response
            udpClient.Client.ReceiveTimeout = timeoutMs;


            IPEndPoint remoteEP = serverEndPoint;// new IPEndPoint(IPAddress.Parse(Config.ANY_ADDRESS), 0);
            byte[] responseData = udpClient.Receive(ref remoteEP);
            BinaryReader reader = new BinaryReader(new MemoryStream(responseData));

                protocal   = reader.ReadInt32();
            int messageUID = reader.ReadInt32();
            int errorCode  = reader.ReadInt32();

            if (errorCode == ErrorCode.None) {
                var sm = Protocals.GetSM(protocal);
                if (sm == null){
                    Logger.Error("Received unknown protocol: " + protocal);
                    return;
                }
                sm.readFrom(reader);

                if (_listeners.ContainsKey(messageUID)) {
                    var ls = _listeners[messageUID];
                    _listeners.Remove(messageUID);
                    //lstodo into unity thread
                    ls(sm);
                }
            }
     
        }

        /// <summary>
        /// 不能throw exception了，在内部处理掉！！
        /// </summary>
        /// <param name="cm"></param>
        public void Send(CM cm)
        {
            Thread thread = new Thread(() => {
                try {
                    lock(_lock){
                        _send(cm);
                    }
                }
                catch (Exception ex) {
                    if (cm.GetType() == typeof(CM_SearchRoom)) ;//搜索房间时不需要报错
                    else Logger.Error("UDP send error: " + ex.Message);
                }
            });
            thread.Start();
        }

        public void Close()
        {
            if (udpClient != null)
            {
                udpClient.Close();
                Logger.Info("UDP client closed");
            }
        }

    }
} 