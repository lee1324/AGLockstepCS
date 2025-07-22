using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AGSyncCS {
    public class CM_HandShake : CM {
        /// <summary>
        /// 目前房主发0是会被server缓存的，其他成员发自己的位置（但不会被server记住）
        /// </summary>
        public int shakeI;
        public override void writeTo(BinaryWriter writer) {
            writer.Write(shakeI);
        }

        public override void readFrom(BinaryReader reader) {
            shakeI = reader.ReadInt32();
        }

        public override string ToString() {
            return string.Format("CM_handshake shakeI:{0}", shakeI);
        }
            
    }

    public class SM_HandShake : SM {
        public int shakeI;

        public override void writeTo(BinaryWriter writer) {
            writer.Write(shakeI);
        }

        public override void readFrom(BinaryReader reader) {
            shakeI = reader.ReadInt32();
        }

        public override string ToString() {
            return string.Format("shakeI:{0}", shakeI);
        }
    }
}
