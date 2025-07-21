using System.IO;

namespace AGSyncCS {

    public class CM_EnterRoom : CM
    {
        //for remote network
        public string userID = "";
        public string userName = "";
        public string roomID = "";//toom to enter

        //local network
        public string memberIP = "";//local IP of the user, used to connect to the room


        public override void writeTo(BinaryWriter writer)
        {
            writer.Write(userID);
            writer.Write(userName);
            writer.Write(roomID);

            writer.Write(memberIP);
        }
        public override void readFrom(BinaryReader reader)
        {
            userID = reader.ReadString();
            userName = reader.ReadString();
            roomID = reader.ReadString();

            memberIP = reader.ReadString();
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