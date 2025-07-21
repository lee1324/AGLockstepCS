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
        Room localRoom = null;
        public void newLocalRoom(string ownerName){
            localRoom = new Room();
            localRoom.roomState = eRoomState.Idle;
            localRoom.startTime = DateTime.Now;
            //set owner
            localRoom.usersNames = new string[Config.MaxPlayersPerRoom]; 
            localRoom.usersIDs = new string[Config.MaxPlayersPerRoom];
            localRoom.usersIPs = new string[Config.MaxPlayersPerRoom];


            localRoom.usersNames[0] = ownerName;
            localRoom.usersIPs[0] = Tools.GetLocalIP();
            //step 14: tell other clients of owner's roomID
            Logger.Instance.Info(string.Format("New local room created by {0} with IP {1}", ownerName, localRoom.usersIPs[0]));
            Logger.Instance.Info("roomID:" + localRoom.getID());
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
        }

        //None in local network/Wifi
        void on(CM_NewRoom cm, ref int errorCode, ref SM sm_response) { }

        void on(CM_EnterRoom cm, ref int errorCode, ref SM sm_response) {

        }

        void on(CM_Test cm, ref int errorCode, ref SM sm_response) {
            var s = new SM_Test();
            s.i1 = cm.i1 + 20;
            s.str1 = "sm.str1 from server:" + cm.str1;
            sm_response = s;//set reponse to client
        }

     
    }
}
