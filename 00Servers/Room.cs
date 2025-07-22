using AGSyncCS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Text;

namespace AGSyncCS {

     public partial class Room {
        public string ID;
        //send all users in this room(self @ 0), sometimes ignore self

        /// <summary>
        /// 0 owner(could be null), 1,2,3 as other members(not null)
        /// </summary>
        public TcpClientConnection[] usersConnections;//used in local wifi
        public string[] usersNames;
        public int[] loadingProgresses0_100;


        public eRoomState roomState;
        public DateTime startTime;


        public void update() {

        }

        public Room() {
            roomState = eRoomState.Idle;
            startTime = DateTime.Now;
            usersConnections = new TcpClientConnection[Config.MaxPlayersPerRoom];

            usersNames = new string[Config.MaxPlayersPerRoom];
            loadingProgresses0_100 = new int[Config.MaxPlayersPerRoom];
        }

        public void resetLoadingProgresses() {
            for(int i = 0; i < loadingProgresses0_100.Length; ++i) {
                loadingProgresses0_100[i] = 0;
            }
        }

        public void printState() {
            for(int i = 1; i < usersConnections.Length; i++) {
                if (usersConnections[i] != null) {
                    Logger.Info(string.Format("member[{0}] name:{1} IP:{2}", i,  usersNames[i],  usersConnections[i].RemoteEndPoint));
                } else {
                    Logger.Info(string.Format("member[{0}] empty", i));
                }
            }
        }

    }



}