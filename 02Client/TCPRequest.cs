using System;
using System.Runtime.CompilerServices;

namespace AGSyncCS
{
    public class TCPRequest
    {
  
        CM cm = null;
 
     
        public TCPRequest setCM(CM m)
        {
            this.cm = m;
            return this;
        }

     

        /// <summary>
        /// part1 global error
        /// part2 custom error(check)
        /// </summary>
        /// <param name="errorCode"></param>
        public void OnError(int errorCode)
        {

        }


    }
}