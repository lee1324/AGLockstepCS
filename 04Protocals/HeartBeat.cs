using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AGSyncCS {

    public class CM_HeartBeat : CM
    {
        public string lastBeatTime;

        public override void writeTo(BinaryWriter writer)
        {
            writer.Write(lastBeatTime);
        }
        public override void readFrom(BinaryReader reader)
        {
            lastBeatTime = reader.ReadString(); 
        }
        public override string ToString() {
            return lastBeatTime;
        }
    }

    public class SM_HeartBeat : SM
    {
        public string lastBeatTime;//send back to client

        public override void writeTo(BinaryWriter writer)
        {
            writer.Write(lastBeatTime);
        }
        public override void readFrom(BinaryReader reader)
        {
            lastBeatTime = reader.ReadString(); 
        }

        public override string ToString() {
            return lastBeatTime;
        }

    }
}
