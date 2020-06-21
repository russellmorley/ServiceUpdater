using System;

namespace Updater
{
    public class ApiException : Exception
    {
        public string Url { get; set; }
        public string Method { get; set; }
        public int StatusCode { get; set; }
        public string Content { get; set; }
        public override string Message { 
            get
            {
                return $"Url: {Url}; Method: {Method}; StatusCode: {StatusCode}; Content: {Content}";
            } 
        }
    }
}
