using AGSyncCS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace AGSyncCS {
    public class RoomsManager {
        Room[] _rooms;
        Dictionary<string, Room> _ownerID2Room;

        private static RoomsManager _instance = null;
        public static RoomsManager Instance { 
            get {
                if(_instance == null) _instance = new RoomsManager();
                return _instance;
            }
        }

        public void printState() {
            var s = "--- RoomsState:\n";
            for(int i = 0; i < Config.MaxRooms; ++i) {
                var r = _rooms[i];
                var userID = string.IsNullOrEmpty(r.usersIDs[0]) ? "none" : r.usersIDs[0];
                s += $"Room {i}/{Config.MaxRooms} state: {r.roomState} ownerID:{userID}\n";
            }
            Logger.Instance.Debug(s);
        }

        public Room newRoom(string ownerID) {
            Room result = null;
            lock(this) {
                if (_ownerID2Room.ContainsKey(ownerID)) {//previous(kept for unknow reason)
                    result = _ownerID2Room[ownerID];
                    //lstodo result.clear
                }
                else {
                    for (int i = 0; i < Config.MaxRooms; ++i) {
                        if (_rooms[i].roomState == eRoomState.Idle) {
                            result = _rooms[i];
                            result.roomState = eRoomState.Waiting;
                            _ownerID2Room[ownerID] = result;
                            break;
                        }
                    }
                }
            }
            return result;
        }
        public Room getRoom(int roomIDX) {
            return roomIDX>=0 && roomIDX < Config.MaxRooms?_rooms[roomIDX]:null;
        }

        public void init() {
            Logger.Instance.Debug("wifi 模式, 不要用此类");
            _rooms = new Room[Config.MaxRooms];
            _ownerID2Room = new Dictionary<string, Room>(Config.MaxRooms);

            for (int i = 0; i < Config.MaxRooms; ++i) {
                var n = new Room();
                //n.ID = i;
                _rooms[i] = n;
            }

            var t = new Thread(() => {
                var DT = (int)(1000/ Config.SyncFPS);
                while (true) {
                    var elapsed = 0;
                    try {
                        var watch = new Stopwatch();
                        _update();
                        watch.Stop();
                        elapsed = (int)watch.ElapsedMilliseconds;
                    } catch (Exception ex) {
                        Logger.Instance.Error("RoomsManager update error: " + ex.Message);
                    }
                    Thread.Sleep(DT - elapsed);
                }


            });
            t.IsBackground = true;
            t.Start();
        }

        void _update() {
            for(int i = 0; i < Config.MaxRooms; ++i) {
                var r = _rooms[i];
                if (r.roomState == eRoomState.Playing) {
                    r.update();
                }
            }
        }
    }
}
