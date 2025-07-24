
using System.Runtime.CompilerServices;

namespace AGSyncCS {

    public class Protocals
    {
        public const int VersionCode =  1;//协议更新版本， 前端获取，和后端对比 由HandShake请求

        public const int None    = 00;//no error, no message,used for init
        public const int TestConnection = 2;//检查tcp ip/port是不是我们的后端（能连的不定是，一定要发个看能否正常回来，就发这个）
        public const int SearchRoom = 10;//udp search room

        public const int Heartbeat = 11;//心跳包，客户端每隔一段时间发送一次，服务器端收到后回复
        public const int ServerError = 12;//错误信息，服务器端发送给客户端
        public const int CloseConnection = 13;//关闭tcp长连接，服务器端发送给客户端

        public const int Test = 20;//test only

        public const int NewRoom   = 51;
        public const int EnterRoom = 52;
        public const int QuitRoom  = 53;//客户端退出房间
        public const int TakePos = 54;
        public const int CancelPos = 55;

        public const int StartLoading = 61;//server push message to clients to start loading
        public const int LoadingProgress = 62;

        public const int Sync = 60;//udp


        /// <summary>
        /// 发送cm前要先发它的protocal, 在这里生成
        /// </summary>
        /// <param name="cm"></param>
        /// <returns></returns>
        //Protocal cm sm are always bounded
        public static int GetProtocal(CM cm)
        {
            int p = None;
            var cmType = cm.GetType();
            if (cmType == typeof(CM_HeartBeat)) p = Heartbeat;
            else if (cmType == typeof(CM_Test)) p = Test;
            else if (cmType == typeof(CM_NewRoom)) p = NewRoom;
            else if (cmType == typeof(CM_EnterRoom)) p = EnterRoom;
            else if (cmType == typeof(CM_QuitRoom)) p = QuitRoom;
            else if (cmType == typeof(CM_LoadingProgress)) p = LoadingProgress;
            else if (cmType == typeof(CM_Sync)) p = Sync;
            else if (cmType == typeof(CM_TestConnection)) p = TestConnection;
            else if (cmType == typeof(CM_SearchRoom)) p = SearchRoom;
            else if (cmType == typeof(CM_TakePos)) p = TakePos;
            else if (cmType == typeof(CM_CancelPos)) p = CancelPos;
            return p;
        }

  
        /// <summary>
        /// server收到message时解析
        /// </summary>
        /// <param name="protocal"></param>
        /// <returns></returns>
        public static CM GetCM(int protocal)
        {
            if (protocal == Heartbeat) return new CM_HeartBeat();
            else if (protocal == Test) return new CM_Test();
            else if (protocal == NewRoom) return new CM_NewRoom();
            else if (protocal == EnterRoom) return new CM_EnterRoom();
            else if (protocal == QuitRoom) return new CM_QuitRoom();
            else if (protocal == LoadingProgress) return new CM_LoadingProgress();
            else if (protocal == Sync) return new CM_Sync();
            else if (protocal == TestConnection) return new CM_TestConnection();
            else if (protocal == SearchRoom) return new CM_SearchRoom();
            else if (protocal == TakePos) return new CM_TakePos();
            else if (protocal == CancelPos) return new CM_CancelPos();
            return null;
        }

        /// <summary>
        /// 前端收到包时解析(push or response)
        /// </summary>
        /// <param name="protocal"></param>
        /// <returns></returns>
        public static SM GetSM(int protocal) {
            if (protocal == Heartbeat) return new SM_HeartBeat();
            else if (protocal == Test) return new SM_Test();
            else if (protocal == NewRoom) return new SM_NewRoom();
            else if (protocal == EnterRoom) return new SM_EnterRoom();
            else if (protocal == QuitRoom) return new SM_QuitRoom();
            else if (protocal == StartLoading) return new SM_StartLoading();
            else if (protocal == LoadingProgress) return new SM_LoadingProgress();
            else if (protocal == Sync) return new SM_Sync();
            else if (protocal == TestConnection) return new SM_TestConnection();
            else if (protocal == SearchRoom) return new SM_SearchRoom();
            else if (protocal == TakePos) return new SM_TakePos();
            else if (protocal == CancelPos) return new SM_CancelPos();
            return null;
        }


        //后端push sm信息时需要
        public static int GetProtocal(SM sm) {
            int p = None;
            var smType = sm.GetType();
            if (smType == typeof(SM_StartLoading)) p = StartLoading;
            else if (smType == typeof(SM_TakePos)) p = TakePos;
            else if (smType == typeof(SM_CancelPos)) p = CancelPos; 
            //PUSH ONLY!!!
            return p;
        }
    }   




}