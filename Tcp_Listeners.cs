using AGSyncCS;
using System;
using System.Collections.Generic;
using System.Linq;
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

        void on(CM_NewRoom cm, ref int errorCode, ref SM sm_response) {
            var room = RoomsManager.Instance.newRoom(cm.userID);
            if (room == null){ //no room available
                errorCode = ErrorCode.NoRoomsAvailable;
            }
            else{
                room.usersIDs[0] = cm.userID;
                room.roomState = eRoomState.Waiting;

                var sm = new SM_NewRoom();
                sm.roomID = room.ID; 
                sm_response = sm;//set reponse to client
            }

            //RoomsManager.Instance.printState();
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
    }
}
