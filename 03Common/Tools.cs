using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace AGSyncCS{
    public class Tools {
        //called by owner
        public static string GetLocalIP() {//last 2 parts of local IP address
            var localIP = "127.0.0.1";
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork) // IPv4
                {
                    var ipStr = ip.ToString();
                    localIP = ipStr;
                }
            }
            return localIP;
        }

        /// <summary>
        /// localIP to RoomID(used by owner)
        /// </summary>
        /// <param name="ownerIP"></param>
        /// <returns></returns>
        public static String IP2RoomID(string ownerIP) {
            var o = "";
            if (ownerIP.Contains(".")) {
                var splits = ownerIP.Split('.');
                int part1 = int.Parse(splits[splits.Length - 2]);
                int part2 = int.Parse(splits[splits.Length - 1]);
                o = string.Format("{0:D3}{1:D3}", part1, part2);
            }
            return o;
        }

        /// <summary>
        /// local RoomID to IP(used by clients)
        /// </summary>
        /// <param name="roomID"></param>
        /// <returns></returns>
        public static string RoomID2IP(string roomID) {
            var part1 = int.Parse(roomID.Substring(0, 3));
            var part2 = int.Parse(roomID.Substring(3, 3));
            var localIP = GetLocalIP();

            //xxx.xxx.007.024 is wrong! remove 0s before 7 and 24!!!
            var splits = localIP.Split('.');
            var IP = string.Format("{0}.{1}.{2}.{3}", splits[0], splits[1], part1, part2);
            return IP;
        }
    }
}
