namespace AGSyncCS {

    public class Protocals
    {
        public const int Version = 0x01;//协议更新版本， 前端获取，和后端对比 由GetVer请求


        public const int GetVer = 0x10;//every time you change a protocal, update this
        public const int NewRoom = 51;
        public const int EnterRoom = 52;

        public const int Disband = 60;


    }   


}