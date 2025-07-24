using System;
using System.Collections.Generic;
using System.Threading;

namespace AGSyncCS {

    /// <summary>
    /// 1 roomId connect directly
    /// 2 search for rooms
    /// </summary>
     public class BandClient {//wrapper of tcp and udp client
        public DateTime dt;//debug only

        public int pos;//position in the room, used to enter the room
        public string roomID = "";//room to enter
        public string nickname = "";//nickname, used in local network

        public BandClient() {
            this.nickname = "BandClient" + pos; //default nickname
        }

        /// <summary>
        /// roomID获取方式：
        /// 1 线下从房主得到（面对面）
        /// 2 搜索得到房间列表，点选其一
        /// </summary>
        /// <param name="roomID"></param>
        /// <param name="onSuccess"></param>
        /// <param name="onFail"></param>
        /// <returns></returns>
        public BandClient enterRoom(string roomID, Action onSuccess, Action onFail = null) {
            this.roomID = roomID;
            var serverIP = Tools.RoomID2IP(roomID);

            //tcp支持动态端口：测几个端口，哪个有回应就用哪个
            for(int i = 0; i < Config.MAX_PORT_RETRY; ++i) {
                var c = new TCPClientWrapper(this);
                int tryPort = Config.TCP_SERVER_PORT + i;

                c.connect(serverIP, tryPort, () => {
                    _tcpClient = c;
                    onSuccess();
                }, (failMessage) => {
                });
            }

            //udp暂不支持动态端口
            _udpClient = new UdpClientWrapper(serverIP, Config.UDP_SERVER_PORT);
            return this;
        }


        List<string> _searchedIP = new List<string>();
        Action<string[]> _onSearchRoomsResult = null;
        /// <summary>
        /// 局域网搜索房间，用udp(not tcp)更快
        /// A.B.C.(0~255) 只遍历搜最后一段
        /// </summary>
        /// <param name="onResult"></param>
        public void searchRooms(Action<string[]> onResult) {
            this._onSearchRoomsResult = onResult;
            _searchedIP.Clear();
            var t = new Thread(_searchRoomsOnce);
            t.Start();
        }

        void _searchRoomsOnce() {
            var localIP = Tools.GetLocalIP();
            var splits = localIP.Split('.');
            var PartABC = splits[0] + "." + splits[1] + "." + splits[2] + ".";


            //255是广播，不要发，不然一定会收回
            //房主广播 成员监听，也能做到“搜房功能”，但是热点和公网无法移植，所以不用此法
            for(int i = 0; i <= 254; ++i) {
                var testIP = PartABC + i;
                //if (testIP == localIP) continue;//self 
                var cm = new CM_SearchRoom();
                cm.str = testIP;//will be echo back

                cm.onResponse = (sm_response) => {
                    var sm = (SM_SearchRoom)sm_response;
                    Logger.Debug("SM_SearchRoom on IP:" + sm.str);
                    if (!_searchedIP.Contains(sm.str))
                        _searchedIP.Add(sm.str);
                };

                var udpClient = new UdpClientWrapper(testIP, Config.UDP_SERVER_PORT);
                udpClient.Send(cm);//send()不要throw exception(内部处理掉exception), 否则会打破这个for -loop
            }
            Thread.Sleep(300);//300ms未连上的也都不考虑了（这么长的延时认为不能玩）

            List<string> o = new List<string>();
            foreach(var ip in _searchedIP) {
                o.Add(Tools.IP2RoomID(ip));
            }
            _onSearchRoomsResult(o.ToArray());
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



        /*用户在实例化当煎类后，就可能立即加Push监听
         * 但tcpClient还未存在（它是在connect成功后才创建的）
         所以把监听放在当前类里
        tcp收到push后，再转接过来调用监听
         
         */
        public void onPush(int protocal, Action<SM> action) {
            _pushListeners[protocal] = action;
        }

        public void dispatchPush(int protocal, SM sm) {
            if (_pushListeners.ContainsKey(protocal)) {
                _pushListeners[protocal](sm);
            }
            else {
                Logger.Error("BandClient dispatchPush: no listener for protocal " + protocal);
            }
        }

        Dictionary<int, Action<SM>> _pushListeners = new Dictionary<int, Action<SM>>();//Protocal - Action

        //band client 内是 TCP 和 UDP 的封装
        TCPClientWrapper _tcpClient = null;
        UdpClientWrapper _udpClient = null;

    }
}
