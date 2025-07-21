using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic; // Added for List

namespace AGSyncCS
{
    public class ComprehensiveServerTest
    {
        public static void RunTest()
        {
            Logger.Info("=== Comprehensive Server Test ===");
            
            // Test HTTP Server
            TestHttpServer();
            
            // Test UDP Server
            TestUdpServer();
            
            // Test both servers simultaneously
            TestBothServersTogether();
            
            Thread.Sleep(2000);
            Logger.Info("=== Comprehensive Server Test Complete ===");
        }
        
        private static void TestHttpServer()
        {
            Logger.Info(string.Format("--- Testing HTTP Server (Port {0}) ---", Config.HTTP_SERVER_PORT));
            
            try
            {
                // Test 1: Status endpoint
                Logger.Info("Test 1: HTTP Status Endpoint");
                string statusResponse = SendHttpRequest(Config.HTTP_SERVER_PORT, "/status");
                Logger.Info("Status Response: " + statusResponse);
                
                // Test 2: Echo endpoint
                Logger.Info("Test 2: HTTP Echo Endpoint");
                string echoResponse = SendHttpRequest(Config.HTTP_SERVER_PORT, "/echo?text=hello");
                Logger.Info("Echo Response: " + echoResponse);
                
                // Test 3: Main page
                Logger.Info("Test 3: HTTP Main Page");
                string mainPageResponse = SendHttpRequest(Config.HTTP_SERVER_PORT, "/");
                Logger.Info("Main Page Response Length: " + mainPageResponse.Length + " characters");
                
                // Test 4: 404 endpoint
                Logger.Info("Test 4: HTTP 404 Endpoint");
                string notFoundResponse = SendHttpRequest(Config.HTTP_SERVER_PORT, "/nonexistent");
                Logger.Info("404 Response: " + notFoundResponse);
                
            }
            catch (Exception ex)
            {
                Logger.Error("HTTP Server Test Error: " + ex.Message);
            }
        }
        
        private static void TestUdpServer()
        {
            Logger.Info(string.Format("--- Testing UDP Server (Port {0}) ---", Config.UDP_SERVER_PORT));
            UdpClientWrapper client = new UdpClientWrapper();
            try
            {
                client.Connect(Config.UDP_HOST, Config.UDP_SERVER_PORT);

                // Test 1: Simple message
                Logger.Info("Test 1: UDP Simple Message");
                string response1 = client.SendAndReceive("UDP Test 1");
                Logger.Info("UDP Response 1: " + response1);

                // Test 2: Multiple messages
                Logger.Info("Test 2: UDP Multiple Messages");
                for (int i = 0; i < 3; i++)
                {
                    string msg = "UDP Message " + i;
                    string response = client.SendAndReceive(msg);
                    Logger.Info("UDP Message: " + msg + " -> Response: " + response);
                }

                // Test 3: Long message
                Logger.Info("Test 3: UDP Long Message");
                string longMessage = new string('a', 1000);
                string longResponse = client.SendAndReceive(longMessage);
                Logger.Info("UDP Long Response: " + longResponse);
            }
            catch (Exception ex)
            {
                Logger.Error("UDP Server Test Error: " + ex.Message);
            }
            finally
            {
                client.Close();
            }
        }
        
        private static void TestBothServersTogether()
        {
            Logger.Info("--- Testing Both Servers Simultaneously ---");
            
            List<Thread> threads = new List<Thread>();
            
            // HTTP threads
            for (int i = 0; i < 5; i++)
            {
                threads.Add(new Thread(() => {
                    try
                    {
                        string response = SendHttpRequest(Config.HTTP_SERVER_PORT, "/echo?text=threadtest" + i);
                        Logger.Info("HTTP Thread Test " + i + ": " + response);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("HTTP Thread Error: " + ex.Message);
                    }
                }));
            }
            
            // UDP threads
            UdpClientWrapper[] udpClients = new UdpClientWrapper[5];
            for (int i = 0; i < 5; i++)
            {
                udpClients[i] = new UdpClientWrapper();
                udpClients[i].Connect(Config.UDP_HOST, Config.UDP_SERVER_PORT);
                int threadNum = i;
                threads.Add(new Thread(() => {
                    try
                    {
                        string response = udpClients[threadNum].SendAndReceive("UDP Thread Test " + threadNum);
                        Logger.Info("UDP Thread Test " + i + ": " + response);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("UDP Thread Error: " + ex.Message);
                    }
                }));
            }
            
            foreach(var t in threads) t.Start();
            foreach(var t in threads) t.Join();

            foreach(var c in udpClients) c.Close();

            Logger.Info("--- Simultaneous Testing Complete ---");
        }
        
        private static string SendHttpRequest(int port, string path)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Config.GetHttpServerUrl() + path);
                request.Method = "GET"; // Default to GET for simplicity, adjust for POST if needed
                request.Timeout = 5000;
                
                // For POST, you'd need to set ContentType and ContentLength
                // and write data to the request stream.
                // This example is simplified for GET.
                
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