using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AGServer
{
    public class ComprehensiveServerTest
    {
        public static void RunTest()
        {
            LogService.Instance.Info("=== Comprehensive Server Test ===");
            
            // Test HTTP Server
            TestHttpServer();
            
            // Test UDP Server
            TestUdpServer();
            
            // Test both servers simultaneously
            TestBothServersSimultaneously();
            
            LogService.Instance.Info("=== Comprehensive Server Test Complete ===");
        }
        
        private static void TestHttpServer()
        {
            LogService.Instance.Info(string.Format("--- Testing HTTP Server (Port {0}) ---", ServerConfig.HTTP_SERVER_PORT));
            
            try
            {
                // Test 1: Status endpoint
                LogService.Instance.Info("Test 1: HTTP Status Endpoint");
                string statusResponse = SendHttpRequest("GET", ServerConfig.GetHttpServerUrl() + "/api/status", null);
                LogService.Instance.Info("Status Response: " + statusResponse);
                
                // Test 2: Echo endpoint
                LogService.Instance.Info("Test 2: HTTP Echo Endpoint");
                string echoResponse = SendHttpRequest("POST", ServerConfig.GetHttpServerUrl() + "/api/echo", "Hello from HTTP test!");
                LogService.Instance.Info("Echo Response: " + echoResponse);
                
                // Test 3: Main page
                LogService.Instance.Info("Test 3: HTTP Main Page");
                string mainPageResponse = SendHttpRequest("GET", ServerConfig.GetHttpServerUrl() + "/", null);
                LogService.Instance.Info("Main Page Response Length: " + mainPageResponse.Length + " characters");
                
                // Test 4: 404 endpoint
                LogService.Instance.Info("Test 4: HTTP 404 Endpoint");
                string notFoundResponse = SendHttpRequest("GET", ServerConfig.GetHttpServerUrl() + "/nonexistent", null);
                LogService.Instance.Info("404 Response: " + notFoundResponse);
                
            }
            catch (Exception ex)
            {
                LogService.Instance.Error("HTTP Server Test Error: " + ex.Message);
            }
        }
        
        private static void TestUdpServer()
        {
            LogService.Instance.Info(string.Format("--- Testing UDP Server (Port {0}) ---", ServerConfig.UDP_SERVER_PORT));
            
            try
            {
                UdpClientWrapper client = new UdpClientWrapper();
                client.Connect(ServerConfig.LOCALHOST, ServerConfig.UDP_SERVER_PORT);
                
                // Test 1: Simple message
                LogService.Instance.Info("Test 1: UDP Simple Message");
                string response1 = client.SendAndReceive("Hello UDP Server!");
                LogService.Instance.Info("UDP Response 1: " + response1);
                
                // Test 2: Multiple messages
                LogService.Instance.Info("Test 2: UDP Multiple Messages");
                string[] messages = { "Test A", "Test B", "Test C" };
                foreach (string msg in messages)
                {
                    string response = client.SendAndReceive(msg);
                    LogService.Instance.Info("UDP Message: " + msg + " -> Response: " + response);
                    Thread.Sleep(200);
                }
                
                // Test 3: Long message
                LogService.Instance.Info("Test 3: UDP Long Message");
                string longMessage = "This is a longer message to test UDP server handling of larger packets. " +
                                   "It contains multiple sentences and should be properly echoed back by the server.";
                string longResponse = client.SendAndReceive(longMessage);
                LogService.Instance.Info("UDP Long Response: " + longResponse);
                
                client.Close();
                
            }
            catch (Exception ex)
            {
                LogService.Instance.Error("UDP Server Test Error: " + ex.Message);
            }
        }
        
        private static void TestBothServersSimultaneously()
        {
            LogService.Instance.Info("--- Testing Both Servers Simultaneously ---");
            
            // Start multiple threads to test both servers at the same time
            Thread httpTestThread = new Thread(() => {
                for (int i = 1; i <= 3; i++)
                {
                    try
                    {
                        string response = SendHttpRequest("GET", ServerConfig.GetHttpServerUrl() + "/api/status", null);
                        LogService.Instance.Info("HTTP Thread Test " + i + ": " + response);
                        Thread.Sleep(500);
                    }
                    catch (Exception ex)
                    {
                        LogService.Instance.Error("HTTP Thread Error: " + ex.Message);
                    }
                }
            });
            
            Thread udpTestThread = new Thread(() => {
                try
                {
                    UdpClientWrapper client = new UdpClientWrapper();
                    client.Connect(ServerConfig.LOCALHOST, ServerConfig.UDP_SERVER_PORT);
                    
                    for (int i = 1; i <= 3; i++)
                    {
                        string response = client.SendAndReceive("Simultaneous Test " + i);
                        LogService.Instance.Info("UDP Thread Test " + i + ": " + response);
                        Thread.Sleep(500);
                    }
                    
                    client.Close();
                }
                catch (Exception ex)
                {
                    LogService.Instance.Error("UDP Thread Error: " + ex.Message);
                }
            });
            
            httpTestThread.Start();
            udpTestThread.Start();
            
            // Wait for both threads to complete
            httpTestThread.Join();
            udpTestThread.Join();
            
            LogService.Instance.Info("--- Simultaneous Testing Complete ---");
        }
        
        private static string SendHttpRequest(string method, string url, string data)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = method;
                request.Timeout = 5000;
                
                if (method == "POST" && !string.IsNullOrEmpty(data))
                {
                    byte[] postData = Encoding.UTF8.GetBytes(data);
                    request.ContentType = "text/plain";
                    request.ContentLength = postData.Length;
                    
                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(postData, 0, postData.Length);
                    }
                }
                
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (var reader = new System.IO.StreamReader(response.GetResponseStream()))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    using (var reader = new System.IO.StreamReader(ex.Response.GetResponseStream()))
                    {
                        return reader.ReadToEnd();
                    }
                }
                throw;
            }
        }
    }
} 