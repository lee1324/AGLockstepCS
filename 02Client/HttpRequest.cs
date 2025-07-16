using System;
using System.Collections.Generic;

namespace AGSyncCS
{
    public class HttpRequest
    {
        public string Method { get; set; }
        public string Path { get; set; }
        public string Version { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string Body { get; set; }

        public HttpRequest()
        {
            Headers = new Dictionary<string, string>();
            Body = "";
        }

        public override string ToString()
        {
            string result = $"{Method} {Path} {Version}\r\n";
            
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