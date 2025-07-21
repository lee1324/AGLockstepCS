using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AGSyncCS {

    public class CM_QuitRoom : CM
    {
        //for remote network
        public int pos = -1;//position in the room, used to enter the room
        public string roomID = "";//room to enter

        public override void writeTo(BinaryWriter writer)
        {
            writer.Write(pos);
            writer.Write(roomID);
        }
        public override void readFrom(BinaryReader reader)
        {
            pos = reader.ReadInt32(); 
            roomID = reader.ReadString();
        }
    }

    public class SM_QuitRoom : SM
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
