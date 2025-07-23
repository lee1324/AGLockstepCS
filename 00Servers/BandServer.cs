using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AGSyncCS {
    /// <summary>
    /// 这是一个管理类外面调用此类接口
    /// </summary>
    public class BandServer {
        public static BandServer Instance = null;

        public BandServer() {
            if (Instance != null)
                Logger.Error("BandServer is Instanced");
            Instance = this;
        }

        TCP_Server _tcpServer = null;
        UDP_Server _udpServer  = null;

        public int portTCP { get { return _tcpServer.port; } }
        public int portUDP { get { return _udpServer.port; } }
        public void start(Action onSuccess, Action<int> onFail) {
            _tcpServer = new TCP_Server();
            _udpServer = new UDP_Server();

            _tcpServer.start(() => {
                Logger.Warning("Tcp server starts successfullly on port:" + _tcpServer.port);
                if (isRunning) onSuccess();
            }, (errorCode) => {
                Logger.Warning("Tcp server starts failed, error:" + errorCode);
                onFail(errorCode);
            });


            _udpServer.start(() => {
                Logger.Debug("Udp Server starts successfully on port:" + _udpServer.port);
                if (isRunning) onSuccess();
            }, (errorCode) => {
                Logger.Warning("Udp server starts failed, error:" + errorCode);
                onFail(errorCode);
            });
        }

        bool isRunning {
            get {
                if (_tcpServer == null || _udpServer == null) {
                    return false;
                }
                if (!_tcpServer.isRunning || !_udpServer.isRunning) {
                    return false;
                }
                return true;
            }
        }


        public void close() {

        }
    }
}
