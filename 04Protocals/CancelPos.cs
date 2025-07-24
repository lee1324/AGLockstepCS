using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO; 

namespace AGSyncCS {
    public class CM_CancelPos : CM　{
        /// <summary>
        /// -1表示未选
        /// </summary>
        public int pos;
        public override void writeTo(System.IO.BinaryWriter writer) {
            writer.Write(pos);
        }

        public override void readFrom(System.IO.BinaryReader reader) {
            pos = reader.ReadInt32();
        }
    }

    public class SM_CancelPos : SM　{
        public int pos;
        //所有被占的位（比如0，1 表示两个位被 占了);
        public int[] posesTaken;
        public override void writeTo(System.IO.BinaryWriter writer) {
            writer.Write(pos);
            int size = posesTaken == null ? 0 : posesTaken.Length;
            writer.Write(size);
            for (int i = 0; i < size; ++i)
                writer.Write(posesTaken[i]);
        }

        public override void readFrom(System.IO.BinaryReader reader) {
            pos = reader.ReadInt32();

            int size = reader.ReadInt32();
            posesTaken = new int[size];
            for (int i = 0; i < size; ++i)
                posesTaken[i] = reader.ReadInt32();
        }

        public override string ToString() {
            int size = posesTaken == null ? 0 : posesTaken.Length;
            var s = "posesTaken size:" + size +" ";
            for (int i = 0; i < size; ++i)
                s += string.Format("[{0}]={1}",i, posesTaken[i]);
            return s;
        }
    }
}
