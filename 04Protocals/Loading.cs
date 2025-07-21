using AGSyncCS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Servers._04Protocals {
   public class CM_StartPlay : CM
    {
        public override void writeTo(BinaryWriter writer)
        {
        }
        public override void readFrom(BinaryReader reader)
        {
        }
    }

    /// <summary>
    /// this is a push message
    /// </summary>
    public class SM_StartPlay : SM
    {
        public override void readFrom(BinaryReader reader)
        {
        }

        public override void writeTo(BinaryWriter writer)
        {
        }

        public override string ToString()
        {
            return string.Format("SM_StartPlay");
        }
    }
}
