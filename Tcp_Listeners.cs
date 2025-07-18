using AGSyncCS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace AGSyncCS {

    partial class TcpClientConnection {

        void dispatch(int protocal, CM cm, ref int errorCode, ref SM sm_response) {
            if(protocal == Protocals.Test)
                on((CM_Test)cm, ref errorCode, ref sm_response);
            else if (protocal == Protocals.NewRoom)
                on((CM_NewRoom)cm, ref errorCode, ref sm_response);
            else if (protocal == Protocals.EnterRoom) 
                on((CM_EnterRoom) cm , ref errorCode, ref sm_response);
        }

        Room localRoom = null;
        void on(CM_NewRoom cm, ref int errorCode, ref SM sm_response) {
            localRoom = new Room();
            localRoom.roomState = eRoomState.Waiting;
            localRoom.ID = getLocalRoomID();
            localRoom.usersIDs[0] = cm.userID;

            var sm = new SM_NewRoom();
            sm.roomID = localRoom.ID;
            sm_response = sm; //set reponse to client
        }

        //void on(CM_ReEnterRoom cm, ref int errorCode, ref SM sm_response) {

        //}

        void on(CM_EnterRoom cm, ref int errorCode, ref SM sm_response) {

        }



        void on(CM_Test cm, ref int errorCode, ref SM sm_response) {
            var s = new SM_Test();
            s.i1 = cm.i1 + 20;
            s.str1 = "sm.str1 from server:" + cm.str1;
            sm_response = s;//set reponse to client
        }

        //called by owner
        string getLocalRoomID() {//last 2 parts of local IP address
            var ID = "0000001";
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork) // IPv4
                {
                    var ipStr = ip.ToString();
                    string[] splits = ip.ToString().Split('.');
                    ID = string.Format("{0:D3}{1:D3}", int.Parse(splits[2]), int.Parse(splits[3]));
                }
            }
            return ID;
        }
    }
}
