using System;
using System.Collections.Generic;

namespace AGSyncCS
{
    public class HttpResponse
    {
        public int StatusCode { get; set; }
        public string StatusText { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string Body { get; set; }

        public HttpResponse()
        {
            StatusCode = 200;
            StatusText = "OK";
            Headers = new Dictionary<string, string>();
            Body = "";
            
            // Default headers
            Headers["Server"] = "SimpleHttpServer/1.0";
            Headers["Date"] = DateTime.Now.ToString("r");
        }

        public override string ToString()
        {
            string result = $"HTTP/1.1 {StatusCode} {StatusText}\r\n";
            
            // Add content length if body is not empty
            if (!string.IsNullOrEmpty(Body))
            {
                Headers["Content-Length"] = Body.Length.ToString();
            }
            
            foreach (var header in Headers)
            {
                result += $"{header.Key}: {header.Value}\r\n";
            }
            
            result += "\r\n";
            result += Body;
            
            return result;
        }
    }
} 