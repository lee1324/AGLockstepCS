using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AGSyncCS {
    public class CM_Sync : CM {
        public string syncData;//js string is recommended.
        public override void writeTo(BinaryWriter writer) {
            writer.Write(syncData);
        }

        public override void readFrom(BinaryReader reader) {
            syncData = reader.ReadString(); 
        }

        public override string ToString() {
            return string.Format("CM_sync.syncData:{0}", syncData);
        }
            
    }

    public class SM_Sync : SM {
        public string syncData;

        public override void writeTo(BinaryWriter writer) {
            writer.Write(syncData);
        }

        public override void readFrom(BinaryReader reader) {
            syncData = reader.ReadString();
        }

        public override string ToString() {
            return string.Format("SM_sync.syncData:{0}", syncData);
        }
    }
}
