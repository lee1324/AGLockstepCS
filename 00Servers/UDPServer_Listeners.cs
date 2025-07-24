using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AGSyncCS {
    partial class UDP_Server {
        void dispatch(int protocal, CM cm, ref int errorCode, ref SM sm_response) {
            if (protocal == Protocals.Sync)
                on((CM_Sync)cm, ref errorCode, ref sm_response);
            else if (protocal == Protocals.TestConnection)
                on((CM_TestConnection)cm, ref errorCode, ref sm_response);
            else if(protocal == Protocals.SearchRoom)
                on((CM_SearchRoom)cm, ref errorCode, ref sm_response);
            else Logger.Warning("udp_server No dispatch:" + protocal);
        }

        public string syncData = "";//last valid syncData;
        void on(CM_Sync cm , ref int errorCode, ref SM sm_response) {
            if(cm.isOwner) {//owner
                syncData = cm.syncData;
            }
            var sm = new SM_Sync();
            sm.syncData = syncData;
            sm_response = sm;
        }

        void on(CM_SearchRoom cm, ref int errorCode, ref SM sm_response) {
            var sm = new SM_SearchRoom();
            sm.str = cm.str;
            sm_response = sm;
        }

        void on(CM_TestConnection cm, ref int errorCode, ref SM sm_response) {
            var sm = new SM_TestConnection();
            sm.shakeI = cm.shakeI * 2;//check protocal
            sm.shakeStr = cm.shakeStr;
            sm_response = sm;
         }



    }
}
