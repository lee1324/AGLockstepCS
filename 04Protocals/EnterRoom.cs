using System.IO;

namespace AGSyncCS {

    public class CM_EnterRoom : CM
    {
        public string userID;
        public string userName;
        public int roomID;//toom to enter


        public override void writeTo(BinaryWriter writer)
        {
            writer.Write(userID);
            writer.Write(userName);
            writer.Write(roomID);
        }
        public override void readFrom(BinaryReader reader)
        {
            userID = reader.ReadString();
            userName = reader.ReadString();
            roomID = reader.ReadInt32();
        }
    }

    public class SM_EnterRoom : SM
    {
        public int roomID;

        public override void readFrom(BinaryReader reader)
        {
            roomID = reader.ReadInt32();
        }

        public override void writeTo(BinaryWriter writer)
        {
            writer.Write(roomID);
        }
    }
}