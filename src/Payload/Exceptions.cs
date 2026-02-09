using Payload.ARM;

namespace Payload.Exceptions
{
    public class UnknownResponse : PayloadError
    {
        public UnknownResponse() { }
        public UnknownResponse(string message, JSONObject response) : base(message, response) { }
    }

    public class BadRequest : PayloadError
    {
        public override int GetCode() { return 400; }
        public BadRequest() { }
        public BadRequest(string message, JSONObject response) : base(message, response) { }
    }

    public class InvalidAttributes : PayloadError
    {
        public override int GetCode() { return 400; }
        public InvalidAttributes() { }
        public InvalidAttributes(string message, JSONObject response) : base(message, response) { }
    }

    public class TransactionDeclined : PayloadError
    {
        public override int GetCode() { return 400; }
        public TransactionDeclined() { }
        public TransactionDeclined(string message, JSONObject response) : base(message, response) { }
    }

    public class Unauthorized : PayloadError
    {
        public override int GetCode() { return 401; }
        public Unauthorized() { }
        public Unauthorized(string message, JSONObject response) : base(message, response) { }
    }

    public class NotPermitted : PayloadError
    {
        public override int GetCode() { return 403; }
        public NotPermitted() { }
        public NotPermitted(string message, JSONObject response) : base(message, response) { }
    }

    public class NotFound : PayloadError
    {
        public override int GetCode() { return 404; }
        public NotFound() { }
        public NotFound(string message, JSONObject response) : base(message, response) { }
    }

    public class TooManyRequests : PayloadError
    {
        public override int GetCode() { return 429; }
        public TooManyRequests() { }
        public TooManyRequests(string message, JSONObject response) : base(message, response) { }
    }

    public class InternalServerError : PayloadError
    {
        public override int GetCode() { return 500; }
        public InternalServerError() { }
        public InternalServerError(string message, JSONObject response) : base(message, response) { }
    }

    public class ServiceUnavailable : PayloadError
    {
        public override int GetCode() { return 503; }
        public ServiceUnavailable() { }
        public ServiceUnavailable(string message, JSONObject response) : base(message, response) { }
    }
}
