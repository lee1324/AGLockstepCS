using System;
using System.Runtime.InteropServices;

namespace AGSyncCS
{
    public static partial class Config
    {
        public static int MaxPlayersPerRoom = 2;
        public static int MaxRooms = 10;
        public static string TCP_HOST = "127.0.0.1";

        public static int HEARTBEAT_INTERVAL = 60000;//30000

        public static string UDP_HOST = "127.0.0.1";
        //public const string TCP_HOST = "47.115.227.169";
        //public const string TCP_HOST = "192.168.6.222";//mac mini 5G
        // Server IP Addresses
        public const string LOCALHOST = "127.0.0.1";
        public const string ANY_ADDRESS = "0.0.0.0";


        /// <summary>
        /// 少数情况下，port被占用了，就加1再尝试，最多重试这么多次
        /// 所以各服务器端口相差 比这个值要大
        /// </summary>
        public const int MAX_PORT_RETRY = 3;

        // TCP Server Configuration
        public const int TCP_SERVER_PORT = 9001;
        public const int TCP_MAX_CONNECTIONS = 5000;
        public const int TCP_CONNECTION_TIMEOUT = 30000;

        // UDP Server Configuration
        public const int UDP_SERVER_PORT = 9055;
        public const bool UDP_ECHO_ENABLED = true;
        


        // HTTP Server Configuration
        public const int HTTP_SERVER_PORT = 9095;
        public const string HTTP_SERVER_URL = "http://localhost:9005";
        
        // Logging Configuration
        public const string LOG_FILE_PATH = "C:\\Users\\ls\\Documents\\AGSyncCS\\server.log";
        public const LogLevel DEFAULT_LOG_LEVEL = LogLevel.Debug;
        public const bool ENABLE_CONSOLE_OUTPUT = true;
        public const bool ENABLE_FILE_OUTPUT = true;
        
        // Test Configuration
        public const int TEST_TIMEOUT_MS = 5000;
        public const int TEST_DELAY_MS = 500;
        
        // Network Configuration
        public const int BUFFER_SIZE = 4096;
        public const int SOCKET_TIMEOUT = 30000;

        public const float SyncFPS = .3f;//30

        // Get formatted URLs for logging
        public static string GetHttpServerUrl()
        {
            return string.Format("http://{0}:{1}", LOCALHOST, HTTP_SERVER_PORT);
        }
        
        public static string GetUdpServerAddress()
        {
            return string.Format("{0}:{1}", LOCALHOST, UDP_SERVER_PORT);
        }
        
        public static string GetTcpServerAddress()
        {
            return string.Format("{0}:{1}", LOCALHOST, TCP_SERVER_PORT);
        }
        
        public static string GetServerInfo()
        {
            return string.Format("HTTP: {0}, UDP: {1}, TCP: {2}", 
                GetHttpServerUrl(), GetUdpServerAddress(), GetTcpServerAddress());
        }
    }
} 