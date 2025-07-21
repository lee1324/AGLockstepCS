using AGSyncCS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace AGSyncCS {

    public partial class TCP_Server{ 
              /// <summary>
        /// use this in local network(created by owner
        /// </summary>
        public Room localRoom = null;
        public void newLocalRoom(string ownerName){
            localRoom = new Room();
            localRoom.roomState = eRoomState.Idle;
            localRoom.startTime = DateTime.Now;
            
            localRoom.ID = Tools.IP2RoomID(Tools.GetLocalIP());
            //step 02: tell other clients of owner's roomID
            Logger.Info("roomID:" + localRoom.ID);
        }

        /// <summary>
        /// Step 05
        /// tell other clients(self included) to load
        /// </summary>
        public void startLoad() {

        }
    }

    partial class TcpClientConnection {

        void dispatch(int protocal, CM cm, ref int errorCode, ref SM sm_response) {
            if(protocal == Protocals.Test)
                on((CM_Test)cm, ref errorCode, ref sm_response);
            else if (protocal == Protocals.NewRoom)
                on((CM_NewRoom)cm, ref errorCode, ref sm_response);
            else if (protocal == Protocals.EnterRoom) 
                on((CM_EnterRoom) cm , ref errorCode, ref sm_response);
            else if (protocal == Protocals.QuitRoom) 
                on((CM_QuitRoom)cm, ref errorCode, ref sm_response);
            
            else if (protocal == Protocals.Heartbeat) {
                on((CM_HeartBeat)cm, ref errorCode, ref sm_response);
            }
            //} else if (protocal == Protocals.ServerError) {
            //    on((CM_ServerError)cm, ref errorCode, ref sm_response);
            //} else if (protocal == Protocals.CloseConnection) {
            //    on((CM_CloseConnection)cm, ref errorCode, ref sm_response);
            //} else {
            //    Logger.Error("Unknown protocal: " + protocal);
            //    errorCode = ErrorCode.UnknownProtocal;
            //}
        }

        void on(CM_HeartBeat cm, ref int errorCode, ref SM sm_response) {
            var sm = new SM_HeartBeat();
            sm.lastBeatTime = cm.lastBeatTime;
            sm_response = sm;
        }

        //None in local network/Wifi
        void on(CM_NewRoom cm, ref int errorCode, ref SM sm_response) { }

        void on(CM_EnterRoom cm, ref int errorCode, ref SM sm_response) {
            var roomID = cm.roomID;
            var localRoom = TCP_Server.Instance.localRoom;
            if (cm.pos <= 0 || cm.pos > Config.MaxPlayersPerRoom) {
                errorCode = ErrorCode.InvalidPosition;//invalid position
                Logger.Error("Invalid position: " + cm.pos);
                return;
            }
            if (roomID == localRoom.ID) {
                //step 03: add user to local room
                if(localRoom.usersConnections[cm.pos] != null &&
                    localRoom.usersConnections[cm.pos].remoteEndPoint.ToString() != this.remoteEndPoint.ToString())
                    errorCode = ErrorCode.PositionOccupied;//position occupied
                else {
                    localRoom.usersConnections[cm.pos] = this ;
                    localRoom.usersNames[cm.pos] = cm.nickname;//set nickname
                    var sm = new SM_EnterRoom();

                    sm.pos = cm.pos;
                    sm.roomID = roomID;
                    sm_response = sm;//set response to client
                }

                Logger.Info(string.Format("User entered local room:{0} pos:{1}", roomID, cm.pos));
            } else {
                errorCode = ErrorCode.RoomNotFound;//room not found
            }
        }

        void on(CM_QuitRoom cm, ref int errorCode, ref SM sm_response) {
            var roomID = cm.roomID;
            var localRoom = TCP_Server.Instance.localRoom;
            if (roomID == localRoom.ID) {
                //step 04: join or remove user from local room
                if (localRoom.usersConnections[cm.pos] != null &&
                    localRoom.usersConnections[cm.pos].remoteEndPoint.ToString() == this.remoteEndPoint.ToString()) {
                    localRoom.usersConnections[cm.pos] = null;
                    var sm = new SM_QuitRoom();
                    sm.pos = cm.pos;
                    sm.roomID = roomID;
                    sm_response = sm;//set response to client
                } else {
                    errorCode = ErrorCode.PositionNotFound;//position not found
                }
            } else {
                errorCode = ErrorCode.RoomNotFound;//room not found
            }
        }

        void on(CM_Test cm, ref int errorCode, ref SM sm_response) {
            var s = new SM_Test();
            s.i1 = cm.i1 + 20;
            s.str1 = "sm.str1 from server:" + cm.str1;
            sm_response = s;//set reponse to client
        }

     
    }
}
