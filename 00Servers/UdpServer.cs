using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AGSyncCS
{
    public class UdpServer
    {
        private int port;
        private UdpClient udpClient;
        private Thread listenThread;
        private bool isRunning;
        private bool echoBack;

        public UdpServer(int port, bool echoBack = false)
        {
            this.port = port;
            this.echoBack = echoBack;
            this.isRunning = false;
        }

        public void Start()
        {
            if (isRunning)
                return;

            udpClient = new UdpClient(port);
            isRunning = true;
            listenThread = new Thread(ListenLoop);
            listenThread.IsBackground = true;
            listenThread.Start();
            Logger.Instance.Info("UDP server started on port " + port);
        }

        public void Stop()
        {
            isRunning = false;
            if (udpClient != null)
            {
                udpClient.Close();
            }
            if (listenThread != null && listenThread.IsAlive)
            {
                listenThread.Join(1000);
            }
            Logger.Instance.Info("UDP server stopped.");
        }

        private void ListenLoop()
        {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(Config.ANY_ADDRESS), 0);
            while (isRunning)
            {
                try
                {
                    byte[] data = udpClient.Receive(ref remoteEP);
                    string message = Encoding.UTF8.GetString(data);
                    Logger.Instance.Info(string.Format("Received from {0}: {1}", remoteEP, message));

                    if (echoBack)
                    {
                        byte[] echoData = Encoding.UTF8.GetBytes("Echo: " + message);
                        udpClient.Send(echoData, echoData.Length, remoteEP);
                        Logger.Instance.Debug(string.Format("Echoed back to {0}", remoteEP));
                    }
                }
                catch (SocketException)
                {
                    // Socket closed, exit loop
                    break;
                }
                catch (Exception ex)
                {
                    Logger.Instance.Error("UDP server error: " + ex.Message);
                }
            }
        }
    }
} 