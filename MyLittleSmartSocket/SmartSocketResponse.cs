using System.Net;

namespace MyLittleSmartSocket
{
    public class SmartSocketResponse
    {
        public HttpStatusCode? StatusCode { get; }
        public string Data { get; }
        public bool Success
        {
            get
            {
                return StatusCode == HttpStatusCode.OK;
            }
        }
        public bool NoPing { get; } = false;

        public SmartSocketResponse(HttpStatusCode httpStatusCode)
        {
            Data = null;
            StatusCode = httpStatusCode;
            NoPing = false;
        }

        private SmartSocketResponse()
        {
            NoPing = true;
            StatusCode = null;
            Data = null;
        }

        public SmartSocketResponse(string data)
        {
            StatusCode = HttpStatusCode.OK;
            Data = data;
            NoPing = false;
        }

        public static SmartSocketResponse NoResponse { get; } = new SmartSocketResponse();
    }
}