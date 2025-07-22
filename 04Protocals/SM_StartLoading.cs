using AGSyncCS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace AGSyncCS {

    /// <summary>
    /// this is a push message
    /// </summary>
    public class SM_StartLoading : SM
    {
        public override void readFrom(BinaryReader reader)
        {
  
        }

        public override void writeTo(BinaryWriter writer)
        {
        }

        public override string ToString()
        {
            return "SM_StartLoading Push Message";
        }
    }
}
