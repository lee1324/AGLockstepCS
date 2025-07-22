using AGSyncCS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace AGSyncCS {
   public class CM_LoadingProgress : CM
    {
        public int pos;
        public int progress0_100;
        public override void writeTo(BinaryWriter writer)
        {
            writer.Write(pos);
            writer.Write(progress0_100);
        }
        public override void readFrom(BinaryReader reader)
        {
            pos = reader.ReadInt32();
            progress0_100 = reader.ReadInt32();
        }
    }

    /// <summary>
    /// this is a push message
    /// </summary>
    public class SM_LoadingProgress : SM
    {
        /// <summary>
        /// 所有玩家的加载总进度
        /// 100表示加载完成
        /// </summary>
        public int[] usersLoadingProgress0_100;
        public override void readFrom(BinaryReader reader)
        {
            int count = reader.ReadInt32();
            usersLoadingProgress0_100 = new int[count];
            for (int i = 0; i < count; i++)
            {
                usersLoadingProgress0_100[i] = reader.ReadInt32();
            }
        }

        public override void writeTo(BinaryWriter writer)
        {
            if (usersLoadingProgress0_100 == null)
            {
                writer.Write(0);
                return;
            }
            writer.Write(usersLoadingProgress0_100.Length);
            foreach (var progress in usersLoadingProgress0_100)
            {
                writer.Write(progress);
            }
        }

        public float getTotalProgress0_1f() {
            float den = 100 * usersLoadingProgress0_100.Length;
            float num = 0f;
            foreach(var v in usersLoadingProgress0_100) {
                num += v;
            }
            var clamp = num/den;
            if (clamp >= 1f) clamp = 1f;
            return clamp;
        }

        /// <summary>
        /// 所有人都加载完成了吗？
        /// </summary>
        /// <returns></returns>
        public bool allCompleted
        {
            get {
                if (usersLoadingProgress0_100 == null || usersLoadingProgress0_100.Length == 0)
                    return false;
                foreach (var progress in usersLoadingProgress0_100)
                {
                    if (progress < 100)
                        return false;
                }
                return true;
            }
        }

        public override string ToString()
        {
            var s = "size:" + usersLoadingProgress0_100.Length + " progress:";
            foreach(var progress in usersLoadingProgress0_100) {
                s += progress + " ";
            }
            return s;
        }
    }
}
