using AGSyncCS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace AGSyncCS
{
    public class CM_Test : CM
    {
        public int i1;
        public string str1;

        public override void readFrom(BinaryReader reader)
        {
            i1 = reader.ReadInt32();
            str1 = reader.ReadString();
        }

        public override void writeTo(BinaryWriter writer)
        {
            writer.Write(i1);
            writer.Write(str1);
        }

        public override string ToString()
        {
            return string.Format("i1:{0} str1:{1}", i1, str1);
        }
    }

    public class SM_Test : SM
    {
        public int i1;
        public string str1;
        public override void readFrom(BinaryReader reader)
        {
            i1 = reader.ReadInt32();
            str1 = reader.ReadString();
        }

        public override void writeTo(BinaryWriter writer)
        {
            writer.Write(i1);
            writer.Write(str1);
        }

        public override string ToString()
        {
            return string.Format("i1:{0} str1:{1}", i1, str1);
        }

    }
}
