using System;

namespace AGServer
{
    public static class ServerConfig
    {
        // Server IP Addresses
        public const string LOCALHOST = "127.0.0.1";
        public const string ANY_ADDRESS = "0.0.0.0";
        
        // HTTP Server Configuration
        public const int HTTP_SERVER_PORT = 9005;
        public const string HTTP_SERVER_URL = "http://localhost:9005";
        
        // UDP Server Configuration
        public const int UDP_SERVER_PORT = 9006;
        public const bool UDP_ECHO_ENABLED = true;
        
        // TCP Server Configuration
        public const int TCP_SERVER_PORT = 9007;
        public const int TCP_MAX_CONNECTIONS = 10;
        public const int TCP_CONNECTION_TIMEOUT = 30000;
        
        // Logging Configuration
        public const string LOG_FILE_PATH = "server.log";
        public const LogLevel DEFAULT_LOG_LEVEL = LogLevel.Info;
        public const bool ENABLE_CONSOLE_OUTPUT = true;
        public const bool ENABLE_FILE_OUTPUT = true;
        
        // Test Configuration
        public const int TEST_TIMEOUT_MS = 5000;
        public const int TEST_DELAY_MS = 500;
        
        // Network Configuration
        public const int BUFFER_SIZE = 4096;
        public const int SOCKET_TIMEOUT = 30000;
        
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