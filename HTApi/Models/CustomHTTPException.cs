using System.Net;

namespace HTApi.Models
{
    public class CustomHTTPException : BadHttpRequestException
    {
        public HttpStatusCode StatusCode { get; set; }
        public List<string> Messages { get; set; }

        public int ErrorCount { get; set; }

        public CustomHTTPException() : base("An error occurred.")
        {
        }

    }
}
