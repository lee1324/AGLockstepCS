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
        public List<TcpClientConnection> usersConnections;//used in local wifi
        public int[] loadingProgresses0_100;

        /// <summary>
        /// 被占用的位置（如0，1，2，3，哪个被 占用了就add进来，取消了就移除掉）
        /// </summary>
        public List<int> posesTaken = null;


        public eRoomState roomState;
        public DateTime startTime;


        public void update() {

        }

        public Room() {
            roomState = eRoomState.Idle;
            startTime = DateTime.Now;
            usersConnections = new List<TcpClientConnection>();

            posesTaken = new List<int>();
        }

        public void resetLoadingProgresses() {
            loadingProgresses0_100 = new int[posesTaken.Count];

            for(int i = 0; i < loadingProgresses0_100.Length; ++i) {
                loadingProgresses0_100[i] = 0;
            }
        }

        public void printState() {
            for(int i = 1; i < usersConnections.Count; i++) {
                if (usersConnections[i] != null) {
                    Logger.Info(string.Format("member[{0}] IP:{1}", i,  usersConnections[i].RemoteEndPoint));
                } else {
                    Logger.Info(string.Format("member[{0}] empty", i));
                }
            }
        }

    }



}