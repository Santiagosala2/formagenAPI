using System.Net;

namespace FormagenAPI.Exceptions
{
    public class ErrorResponse
    {
        public string Message { get; set; } = null!;
        public HttpStatusCode StatusCode { get; set; }
    }
}
