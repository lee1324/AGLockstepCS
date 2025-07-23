using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AGSyncCS {
    public class ServerBase {//base of tcp and udp server
        public int port {  get{return _port; } }
        public bool isRunning { get { return _isRunning; } }//0: not running, 1: running

        protected int PortBase = -2;//init in constructor
        protected int _port = -1;
        protected bool _isRunning = false;

        public virtual void start(Action onSuccess, Action<int> onFail) {
            if (PortBase == -2) {
                Logger.Error("Set PortBase in constructor of derived class !!!");
                return;
            }
            _port = PortBase;//start coule be called multiple times, so reset port

            if (_isRunning)
                return;
            //find available port
            int retryTimes = 0;
            while(!_isRunning){
                try {
                    if(retryTimes > 0) 
                        Logger.Warning("start tcp retry times:" + retryTimes + " port:" + _port);
                    tryPort();
                    _isRunning = true;
                    if (onSuccess != null) 
                        onSuccess();
                }
                catch (Exception ex) {//Port taken
                    Logger.Warning(string.Format("Failed to start {0} server:{1}", this.GetType(), ex.Message));
                    if(retryTimes >= Config.MAX_PORT_RETRY) {
                        if(onFail != null) onFail(ErrorCode.PortTaken);
                        break;
                    }
                    else {
                        ++retryTimes;
                        _port = PortBase + retryTimes;//retry on different port
                    }
                }
            }
        }

        /// <summary>
        /// 以异常判断成功与否，所以子类初始化失败了必须抛出异常
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        protected virtual void tryPort() {
            throw new NotImplementedException("tryPort method must be implemented in derived class");
        }
    }
}
