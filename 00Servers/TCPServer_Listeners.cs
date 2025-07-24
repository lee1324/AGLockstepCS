using AGSyncCS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AGSyncCS {
    public partial class TCP_Server{ 
        /// <summary>
        /// use this in local network(created by owner
        /// </summary>
        public Room room = null;
  
        /// <summary>
        /// Step 05
        /// tell other clients(self included) to load
        /// </summary>
        public void notifyStartLoading() {
            room.resetLoadingProgresses();
            push(new SM_StartLoading());
        }

        public void push(SM sm) {
             foreach(var connection in activeConnections) 
                connection.push(sm);
        }
    }

    partial class TcpClientConnection {

        void dispatch(int protocal, CM cm, ref int errorCode, ref SM sm_response) {
            if (protocal == Protocals.Test)
                on((CM_Test)cm, ref errorCode, ref sm_response);
            else if (protocal == Protocals.NewRoom)
                on((CM_NewRoom)cm, ref errorCode, ref sm_response);
            else if (protocal == Protocals.EnterRoom)
                on((CM_EnterRoom)cm, ref errorCode, ref sm_response);

            else if (protocal == Protocals.QuitRoom)
                on((CM_QuitRoom)cm, ref errorCode, ref sm_response);
            else if (protocal == Protocals.Heartbeat)
                on((CM_HeartBeat)cm, ref errorCode, ref sm_response);
            else if (protocal == Protocals.LoadingProgress)
                on((CM_LoadingProgress)cm, ref errorCode, ref sm_response);

            else if (protocal == Protocals.TestConnection)
                on((CM_TestConnection)cm, ref errorCode, ref sm_response);
            else if (protocal == Protocals.TakePos)
                on((CM_TakePos)cm, ref errorCode, ref sm_response);
            else if (protocal == Protocals.CancelPos)
                on((CM_CancelPos)cm, ref errorCode, ref sm_response);
            else if (protocal == Protocals.Sync)
                ;//udp message ignore here on((CM_Sync)cm, ref errorCode, ref sm_response);


            else Logger.Warning("tcp server No dispatch:" + protocal);
        }

        void on(CM_HeartBeat cm, ref int errorCode, ref SM sm_response) {
            var sm = new SM_HeartBeat();
            sm.lastBeatTime = cm.lastBeatTime;
            sm_response = sm;
        }

        /// <summary>
        /// tcp 连通不定有效，必须要发一个包试试是否是我们的后端（就发这个握手包试）
        /// </summary>
        /// <param name="cm"></param>
        /// <param name="errorCode"></param>
        /// <param name="sm_response"></param>
        void on(CM_TestConnection cm, ref int errorCode, ref SM sm_response) {
            var sm = new SM_TestConnection();
            sm.shakeI = cm.shakeI * 2;//check protocal
            sm.shakeStr = cm.shakeStr;
            sm_response = sm;
        }

        void on(CM_TakePos cm, ref int errorCode, ref SM sm_response) {
            var localRoom = server.room;
            if (localRoom.posesTaken.Contains(cm.pos)) 
                errorCode = ErrorCode.PositionOccupied;
            else {
                var sm = new SM_TakePos();
                localRoom.posesTaken.Add(cm.pos);
                sm.pos = cm.pos;
                sm.posesTaken = localRoom.posesTaken.ToArray();
                sm_response = sm;

                //push to all clients
                server.push(sm);
            }
        }

        void on(CM_CancelPos cm, ref int errorCode, ref SM sm_response) {
            var localRoom = server.room;
            if (localRoom.posesTaken.Contains(cm.pos))
                localRoom.posesTaken.Remove(cm.pos);
            else
                Logger.Debug("CancelPos on a empty one:" + cm.pos);

          
            var sm = new SM_CancelPos();
            sm.pos = cm.pos;
            sm.posesTaken = localRoom.posesTaken.ToArray();
            sm_response = sm;

            //push to all clients
            server.push(sm);
        }

        //None in local network/Wifi
        void on(CM_NewRoom cm, ref int errorCode, ref SM sm_response) { }

        void on(CM_EnterRoom cm, ref int errorCode, ref SM sm_response) {
            var roomID = cm.roomID;
            var localRoom = server.room;
    
            if (roomID == localRoom.ID) {
                localRoom.usersConnections.Add(this) ;
                //localRoom.usersNames[cm.pos] = cm.nickname;//set nickname
                var sm = new SM_EnterRoom();

                sm.roomID = roomID;
                sm_response = sm;//set response to client
                Logger.Debug(string.Format("User entered local room:{0} ", roomID));
            } else {
                errorCode = ErrorCode.RoomNotFound;//room not found
            }
        }

        void on(CM_QuitRoom cm, ref int errorCode, ref SM sm_response) {
            var roomID = cm.roomID;
            var localRoom = server.room;
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

        void on(CM_LoadingProgress cm, ref int errorCode, ref SM sm_response) {
            Logger.Debug("on cm_loadingprogress, pos:" + cm.pos + " p:" + cm.progress0_100);
            if(cm.pos < 0 ||
                cm.pos >= _bandServers.room.loadingProgresses0_100.Length) {
                errorCode = ErrorCode.InvalidPosIndex;
            }
            else { 

                var sm = new SM_LoadingProgress();
                _bandServers.room.loadingProgresses0_100[cm.pos] = cm.progress0_100;

                sm.usersLoadingProgress0_100 = new int[_bandServers.room.posesTaken.Count] ;
                for(int i = 0; i < sm.usersLoadingProgress0_100.Length; ++i) {
                    sm.usersLoadingProgress0_100[i] = server.room.loadingProgresses0_100[i];
                }
                sm_response = sm;

                if (sm.allCompleted) {
                }
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
