using System.IO;

namespace AGSyncCS {

    public class CM_EnterRoom : CM
    {
        public string userID;
        public int roomID;//toom to enter

        public override void readFrom(BinaryReader reader)
        {
            userID = reader.ReadString();
            roomID = reader.ReadInt32();
        }

        public override void writeTo(BinaryWriter writer)
        {
            writer.Write(userID);
            writer.Write(roomID);
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