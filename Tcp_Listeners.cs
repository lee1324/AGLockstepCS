using AGSyncCS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AGSyncCS {

    partial class TcpClientConnection {

        void dispatch(int protocal, CM cm, ref SM sm_response) {
            int errorCode = ErrorCode.None;//will be sent to client
            if(protocal == Protocals.Test)
                on((CM_Test)cm, ref errorCode, ref sm_response);
            else if (protocal == Protocals.NewRoom)
                on((CM_NewRoom)cm, ref errorCode, ref sm_response);
            else if (protocal == Protocals.EnterRoom) 
                on((CM_EnterRoom) cm , ref errorCode, ref sm_response);
        }
        public void on(CM_Test cm, ref int errorCode, ref SM sm_response) {
            Logger.Instance.Info("server get cm_test:" + cm.ToString());
            var s = new SM_Test();
            s.i1 = cm.i1 + 20;
            s.str1 = "sm.str1 from server:" + cm.str1;
            sm_response = s;//set reponse to client
        }
        public void on(CM_NewRoom cm, ref int errorCode, ref SM sm_response) {

        }

        public void on(CM_EnterRoom cm, ref int errorCode, ref SM sm_response) {

        }
    }
}
