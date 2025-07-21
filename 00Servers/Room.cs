using AGSyncCS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;

namespace AGSyncCS {

    public partial class Room {
        /// <summary>
        /// [0] -> owner
        /// </summary>
        public string[] usersIDs;
        public string[] usersNames;

        public string[] usersIPs;//used in local wifi
        public eRoomState roomState;
        public DateTime startTime;

        /// <summary>
        /// roomID(6 digits string)
        /// </summary>
        /// <returns></returns>
        public string getID() {
            var o = Tools.IP2RoomID(usersIPs[0]);
            return o;
        }

        public void update() {

        }


        public Room() {
            roomState = eRoomState.Idle;
            startTime = DateTime.Now;

            usersIDs = new string[Config.MaxPlayersPerRoom];
            usersNames = new string[Config.MaxPlayersPerRoom];
            for(int i = 0; i < Config.MaxPlayersPerRoom; ++i) {
                usersIDs[i] = "";
                usersNames[i] = "";
            }
        }

    }



}