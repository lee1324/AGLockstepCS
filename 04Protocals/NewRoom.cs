using System.IO;
namespace AGSyncCS {
    public class CM_NewRoom : CM
    {
        public string userID;
        public override void writeTo(BinaryWriter writer) {
            writer.Write(userID);
        }

        public override void readFrom(BinaryReader reader) {
            userID = reader.ReadString();
        }

        public override string ToString() {
            return "userID:" + userID;
        }
            
    }

    public class SM_NewRoom : SM
    {
        public int roomID;

        public override void writeTo(BinaryWriter writer) {
            writer.Write(roomID);
        }

        public override void readFrom(BinaryReader reader) {
            roomID = reader.ReadInt32();
        }

        public override string ToString() {
            return string.Format("roomID:" + roomID);
        }
    }
}