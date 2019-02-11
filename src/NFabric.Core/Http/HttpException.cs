namespace NFabric.Core.Http
{
    using System;
    using System.Net;
    using System.Runtime.Serialization;

    public class HttpException : Exception
    {
        public HttpStatusCode? StatusCode { get; }

        public byte[] ResponseBody { get; }

        public HttpException()
        {
        }

        public HttpException(string message)
            : base(message)
        {
        }

        public HttpException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected HttpException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public HttpException(HttpStatusCode statusCode, byte[] responseBody = null)
            : base()
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }

        public HttpException(HttpStatusCode statusCode, string message, byte[] responseBody = null)
            : base(message)
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }

        public HttpException(HttpStatusCode statusCode, string message, Exception innerException, byte[] responseBody = null)
            : base(message, innerException)
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }
    }
}
