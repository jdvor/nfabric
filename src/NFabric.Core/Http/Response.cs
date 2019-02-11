namespace NFabric.Core.Http
{
    using System;
    using System.Net;
    using System.Runtime.Serialization;

    [DataContract]
    public sealed class Response<T>
        where T : class
    {
        [DataMember(Name = "isSuccess", Order = 1, IsRequired = true, EmitDefaultValue = true)]
        public bool IsSuccess { get; }

        [DataMember(Name = "value", Order = 2, IsRequired = false, EmitDefaultValue = false)]
        public T Value { get; }

        [DataMember(Name = "exception", Order = 3, IsRequired = false, EmitDefaultValue = false)]
        public Exception Exception { get; }

        [DataMember(Name = "status", Order = 4, IsRequired = false, EmitDefaultValue = false)]
        public HttpStatusCode? Status { get; }

        [DataMember(Name = "error", Order = 5, IsRequired = false, EmitDefaultValue = false)]
        public string Error { get; }

        [IgnoreDataMember]
        public bool HasException => Exception != null;

        [IgnoreDataMember]
        public bool HasValue => Value != null && !(Value is Nothing);

        [IgnoreDataMember]
        public bool HasError => !string.IsNullOrEmpty(Error);

        [IgnoreDataMember]
        public bool HasStatus => Status.HasValue && Status.Value != default(HttpStatusCode);

        public Response(HttpStatusCode status, T value)
        {
            Status = status;
            Value = value;
            IsSuccess = true;
        }

        public Response(HttpStatusCode status, string error)
        {
            Status = status;
            Error = error;
            IsSuccess = false;
        }

        public Response(Exception exception, string error)
        {
            Exception = exception;
            Error = error;
            IsSuccess = false;
        }

        public Response(string error)
        {
            Error = error;
            IsSuccess = false;
        }

        public Response(Exception exception)
        {
            Exception = exception;
            Error = exception.Message;
            IsSuccess = false;
        }
    }
}
