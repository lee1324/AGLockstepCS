using System;
using System.Threading;

namespace AGSyncCS
{
    public class UDP_CS_Test
    {
        public static void RunTest()
        {
            UdpServer udpServer = new UdpServer(9002, true); // Echo back enabled
            udpServer.Start();


            //udpServer.Stop();
        }
    }
} 