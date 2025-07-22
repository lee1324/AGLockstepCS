
namespace AGSyncCS {

    public class Protocals
    {
        public const int None    = 00;//no error, no message,used for init
        public const int Version =  1;//协议更新版本， 前端获取，和后端对比 由GetVer请求
        public const int GetVer  = 10;//every time you change a protocal, update this

        public const int Heartbeat = 11;//心跳包，客户端每隔一段时间发送一次，服务器端收到后回复
        public const int ServerError = 12;//错误信息，服务器端发送给客户端
        public const int CloseConnection = 13;//关闭tcp长连接，服务器端发送给客户端

        public const int Test = 20;//test only

        public const int NewRoom   = 51;
        public const int EnterRoom = 52;
        public const int QuitRoom  = 53;//客户端退出房间

        public const int StartLoading = 54;//server push message to clients to start loading
        public const int LoadingProgress = 55;


        /// <summary>
        /// 发送cm前要先发它的protocal
        /// </summary>
        /// <param name="cm"></param>
        /// <returns></returns>
        //Protocal cm sm are always bounded
        public static int GetProtocal(CM cm)
        {
            int p = None;
            var cmType = cm.GetType();
            if (cmType == typeof(CM_HeartBeat)) p = Heartbeat;
            else if(cmType == typeof(CM_Test)) p = Test;
            else if(cmType == typeof(CM_NewRoom)) p = NewRoom;
            else if(cmType == typeof(CM_EnterRoom)) p = EnterRoom;
            else if(cmType == typeof(CM_QuitRoom)) p = QuitRoom;
            else if(cmType == typeof(CM_LoadingProgress)) p = LoadingProgress;
            return p;
        }

        //for push only
        public static int GetProtocal(SM sm) {
            int p = None;
            var smType = sm.GetType();
            if(smType == typeof(SM_StartLoading)) p = StartLoading;
            return p;
        }

        public static CM GetCM(int protocal)
        {
            if (protocal == Heartbeat) return new CM_HeartBeat();
            else if (protocal == Test) return new CM_Test();
            else if (protocal == NewRoom) return new CM_NewRoom();
            else if (protocal == EnterRoom) return new CM_EnterRoom();
            else if (protocal == QuitRoom) return new CM_QuitRoom();
            else if (protocal == LoadingProgress) return new CM_LoadingProgress();
            return null;
        }

        public static SM GetSM(int protocal) {
            if (protocal == Heartbeat) return new SM_HeartBeat();
            else if (protocal == Test) return new SM_Test();
            else if (protocal == NewRoom) return new SM_NewRoom();
            else if (protocal == EnterRoom) return new SM_EnterRoom();
            else if (protocal == QuitRoom) return new SM_QuitRoom();
            else if (protocal == StartLoading) return new SM_StartLoading();
            else if (protocal == LoadingProgress) return new SM_LoadingProgress();
            return null;
        }
    }   




}