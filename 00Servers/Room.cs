using AGSyncCS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace AGSyncCS {

    public partial class Room {
        public string ID;
        /// <summary>
        /// [0] -> owner
        /// </summary>
        public string[] usersIDs;
        public string[] usersNames;

        public string[] userIPs;//used in local wifi
        public eRoomState roomState;

        public DateTime startTime;

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