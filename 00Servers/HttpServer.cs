using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AGSyncCS
{
    public class HttpServer
    {
        private TcpListener listener;
        private bool isRunning;
        private int port;
        private Thread serverThread;

        public HttpServer(int port)
        {
            this.port = port;
            this.isRunning = false;
        }

        public void Start()
        {
            if (isRunning)
                return;

            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            isRunning = true;

            serverThread = new Thread(ListenForClients);
            serverThread.Start();
        }

        public void Stop()
        {
            isRunning = false;
            if (listener != null)
            {
                listener.Stop();
            }
            if (serverThread != null && serverThread.IsAlive)
            {
                serverThread.Join();
            }
        }

        private void ListenForClients()
        {
            Logger.Info("HTTP Server started listening on port " + port);
            
            while (isRunning)
            {
                try
                {
                    TcpClient client = listener.AcceptTcpClient();
                    Logger.Debug("New client connected: " + client.Client.RemoteEndPoint);
                    
                    Thread clientThread = new Thread(HandleClient);
                    clientThread.Start(client);
                }
                catch (Exception ex)
                {
                    if (isRunning)
                    {
                        Logger.Error("Error accepting client: " + ex.Message);
                    }
                }
            }
        }

        private void HandleClient(object obj)
        {
            TcpClient client = (TcpClient)obj;
            NetworkStream stream = client.GetStream();
            string clientAddress = client.Client.RemoteEndPoint.ToString();

            try
            {
                byte[] buffer = new byte[4096];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string request = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                HttpRequest httpRequest = ParseRequest(request);
                Logger.Info(string.Format("Request from {0}: {1} {2}", 
                    clientAddress, httpRequest.Method, httpRequest.Path));

                HttpResponse httpResponse = ProcessRequest(httpRequest);

                string responseString = httpResponse.ToString();
                byte[] responseBytes = Encoding.ASCII.GetBytes(responseString);
                stream.Write(responseBytes, 0, responseBytes.Length);
                
                Logger.Debug(string.Format("Response to {0}: {1} {2}", 
                    clientAddress, httpResponse.StatusCode, httpResponse.StatusText));
            }
            catch (Exception ex)
            {
                Logger.Error("Error handling client " + clientAddress + ": " + ex.Message);
            }
            finally
            {
                stream.Close();
                client.Close();
                Logger.Debug("Client disconnected: " + clientAddress);
            }
        }

        private HttpRequest ParseRequest(string request)
        {
            HttpRequest httpRequest = new HttpRequest();
            
            string[] lines = request.Split('\n');
            if (lines.Length > 0)
            {
                string[] firstLine = lines[0].Trim().Split(' ');
                if (firstLine.Length >= 3)
                {
                    httpRequest.Method = firstLine[0];
                    httpRequest.Path = firstLine[1];
                    httpRequest.Version = firstLine[2];
                }
            }

            // Parse headers
            bool inHeaders = true;
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line))
                {
                    inHeaders = false;
                    continue;
                }

                if (inHeaders)
                {
                    int colonIndex = line.IndexOf(':');
                    if (colonIndex > 0)
                    {
                        string key = line.Substring(0, colonIndex).Trim();
                        string value = line.Substring(colonIndex + 1).Trim();
                        httpRequest.Headers[key] = value;
                    }
                }
                else
                {
                    // Body content
                    httpRequest.Body += line + "\n";
                }
            }

            return httpRequest;
        }

        private HttpResponse ProcessRequest(HttpRequest request)
        {
            HttpResponse response = new HttpResponse();

            switch (request.Method.ToUpper())
            {
                case "GET":
                    HandleGetRequest(request, response);
                    break;
                case "POST":
                    HandlePostRequest(request, response);
                    break;
                default:
                    response.StatusCode = 405;
                    response.StatusText = "Method Not Allowed";
                    response.Body = "<html><body><h1>405 Method Not Allowed</h1></body></html>";
                    break;
            }

            return response;
        }

        private void HandleGetRequest(HttpRequest request, HttpResponse response)
        {
            if (request.Path == "/" || request.Path == "/index.html")
            {
                response.StatusCode = 200;
                response.StatusText = "OK";
                response.Headers["Content-Type"] = "text/html";
                response.Body = GetDefaultHtml();
            }
            else if (request.Path.StartsWith("/api/"))
            {
                HandleApiRequest(request, response);
            }
            else
            {
                response.StatusCode = 404;
                response.StatusText = "Not Found";
                response.Body = "<html><body><h1>404 Not Found</h1></body></html>";
            }
        }

        private void HandlePostRequest(HttpRequest request, HttpResponse response)
        {
            if (request.Path == "/api/echo")
            {
                response.StatusCode = 200;
                response.StatusText = "OK";
                response.Headers["Content-Type"] = "application/json";
                response.Body = $"{{\"message\": \"Echo response\", \"data\": \"{request.Body.Trim()}\"}}";
            }
            else
            {
                response.StatusCode = 404;
                response.StatusText = "Not Found";
                response.Body = "<html><body><h1>404 Not Found</h1></body></html>";
            }
        }

        private void HandleApiRequest(HttpRequest request, HttpResponse response)
        {
            if (request.Path == "/api/status")
            {
                response.StatusCode = 200;
                response.StatusText = "OK";
                response.Headers["Content-Type"] = "application/json";
                response.Body = "{\"status\": \"running\", \"timestamp\": \"" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\"}";
            }
            else
            {
                response.StatusCode = 404;
                response.StatusText = "Not Found";
                response.Body = "<html><body><h1>404 Not Found</h1></body></html>";
            }
        }

        private string GetDefaultHtml()
        {
            return @"<!DOCTYPE html>
<html>
<head>
    <title>Simple HTTP Server</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 40px; }
        .container { max-width: 800px; margin: 0 auto; }
        .endpoint { background: #f5f5f5; padding: 10px; margin: 10px 0; border-radius: 5px; }
        .method { font-weight: bold; color: #0066cc; }
    </style>
</head>
<body>
    <div class='container'>
        <h1>Simple HTTP Server</h1>
        <p>Welcome to the simple HTTP server built with C# and .NET 2.0 grammar!</p>
        
        <h2>Available Endpoints:</h2>
        
        <div class='endpoint'>
            <span class='method'>GET</span> / - This page
        </div>
        
        <div class='endpoint'>
            <span class='method'>GET</span> /api/status - Server status
        </div>
        
        <div class='endpoint'>
            <span class='method'>POST</span> /api/echo - Echo back posted data
        </div>
        
        <h2>Test the API:</h2>
        <form id='echoForm'>
            <input type='text' id='message' placeholder='Enter a message' style='width: 300px; padding: 5px;'>
            <button type='submit' style='padding: 5px 15px;'>Send</button>
        </form>
        <div id='result'></div>
        
        <script>
            document.getElementById('echoForm').onsubmit = function(e) {
                e.preventDefault();
                var message = document.getElementById('message').value;
                var xhr = new XMLHttpRequest();
                xhr.open('POST', '/api/echo', true);
                xhr.setRequestHeader('Content-Type', 'text/plain');
                xhr.onreadystatechange = function() {
                    if (xhr.readyState === 4) {
                        document.getElementById('result').innerHTML = '<pre>' + xhr.responseText + '</pre>';
                    }
                };
                xhr.send(message);
            };
        </script>
    </div>
</body>
</html>";
        }
    }
} 