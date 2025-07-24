using System.IO;

namespace AGSyncCS {

    public class CM_EnterRoom : CM
    {
        public string roomID = "";
        public string nickname = "";//nickname of the user, used in local network


        public override void writeTo(BinaryWriter writer)
        {
            writer.Write(roomID);
            writer.Write(nickname);
        }
        public override void readFrom(BinaryReader reader)
        {
            roomID = reader.ReadString();
            nickname = reader.ReadString();
        }
    }

    public class SM_EnterRoom : SM
    {
        public int pos;//send backto client
        public string roomID;//send back to client

        public override void readFrom(BinaryReader reader)
        {
            pos = reader.ReadInt32();
            roomID = reader.ReadString();
        }

        public override void writeTo(BinaryWriter writer)
        {
            writer.Write(pos);
            writer.Write(roomID);
        }

        public override string ToString()
        {
            return string.Format("SM_EnterRoom: pos={0}, roomID={1}", pos, roomID);
        }
    }
}