using AGSyncCS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace AGSyncCS {

    public partial class Room {
        public int ID;//id(x), be set in roomsManager
        /// <summary>
        /// [0] -> owner
        /// </summary>
        public string[] usersIDs;
        public string[] usersNames;
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