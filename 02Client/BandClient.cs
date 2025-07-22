using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AGSyncCS {

     public class BandClient {//wrapper of tcp and udp client

        public int pos;//position in the room, used to enter the room
        public string roomID = "";//room to enter
        public string nickname = "";//nickname, used in local network

        public BandClient(string roomID, int pos) {
            this.roomID = roomID;
            this.pos = pos;
            this.nickname = "BandClient" + pos; //default nickname
        }

        public BandClient start() {
             var serverIP = Tools.RoomID2IP(roomID);
            _tcpClient = new TCPClientWrapper().start(serverIP, Config.TCP_SERVER_PORT);
            _udpClient = new UdpClientWrapper(serverIP, Config.UDP_SERVER_PORT);
            return this;
        }
        public void send(CM cm) {
            if(cm.GetType() == typeof(CM_Sync)) {
                _udpClient.Send(cm);
            }
            else _tcpClient.Send(cm);
        }
        public void close() {
            if (_tcpClient == null) ;
            else _tcpClient.Close();

            if (_udpClient == null) ;
            else  _udpClient.Close();
        }

        public void onPush(int protocal, Action<SM> onResponse) {
            if(_tcpClient == null)
                Logger.Warning("onPush called but _tcpClient is null");
            else _tcpClient.onPush(protocal, onResponse);
        }

        //band client 内是 TCP 和 UDP 的封装
        TCPClientWrapper _tcpClient = null;
        UdpClientWrapper _udpClient = null;
    }
}
