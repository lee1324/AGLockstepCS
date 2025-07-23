using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AGSyncCS {

    public class CM_SearchRoom : CM {
        public string str = "";

        public override void writeTo(BinaryWriter writer) {
            writer.Write(str);
        }

        public override void readFrom(BinaryReader reader) {
            str = reader.ReadString();
        }

        public override string ToString() {
            return string.Format("str:{0}", str);
        }
            
    }

    public class SM_SearchRoom : SM {
        public string str;
        
        public override void writeTo(BinaryWriter writer) {
            writer.Write(str);
        }

        public override void readFrom(BinaryReader reader) {
            str = reader.ReadString();
        }

        public override string ToString() {
            return string.Format("str:{0}", str);
        }
    }
}
