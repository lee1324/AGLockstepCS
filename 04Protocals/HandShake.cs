using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AGSyncCS {
    /// <summary>
    /// 用此协议检查server通不通，是不是我们的server（可用做搜索)
    /// </summary>
    public class CM_TestServer : CM {
        /// <summary>
        /// 目前房主发0是会被server缓存的，其他成员发自己的位置（但不会被server记住）
        /// </summary>
        public int shakeI;
        public string shakeStr = "";

        public override void writeTo(BinaryWriter writer) {
            writer.Write(shakeI);
            writer.Write(shakeStr);
        }

        public override void readFrom(BinaryReader reader) {
            shakeI = reader.ReadInt32();
            shakeStr = reader.ReadString();
        }

        public override string ToString() {
            return string.Format("handshake shakeI:{0} str:{1}", shakeI, shakeStr);
        }
            
    }

    public class SM_TestServer : SM {
        public int shakeI;
        public string shakeStr;
        
        public override void writeTo(BinaryWriter writer) {
            writer.Write(shakeI);
            writer.Write(shakeStr);
        }

        public override void readFrom(BinaryReader reader) {
            shakeI = reader.ReadInt32();
            shakeStr = reader.ReadString();
        }

        public override string ToString() {
            return string.Format("handshake shakeI:{0} str:{1}", shakeI, shakeStr);
        }
    }
}
