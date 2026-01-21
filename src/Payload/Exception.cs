using System;
using Newtonsoft.Json;
using Payload.ARM;

namespace Payload
{
    interface IPayloadError
    {
        int GetCode();
    }
    public class PayloadError : Exception, IPayloadError
    {
        public JSONObject Response;
        public dynamic Details;

        public virtual int GetCode()
        {
            return 0;
        }

        public PayloadError() { }

        public PayloadError(string message, JSONObject response) : base(message)
        {
            Response = response;
            Details = Response.HasObject("details") ? Response["details"] : null;
        }

        public string json()
        {
            return Response.Json();
        }
    }
}
