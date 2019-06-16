using System;
using Newtonsoft.Json;
using Payload.ARM;

namespace Payload {
	interface IPayloadError {
		int GetCode();
	}
	public class PayloadError : Exception, IPayloadError {
		public ARMObject<object> response;
		public dynamic details;

		public virtual int GetCode() {
			return 0;
		}

		public PayloadError() {}

		public PayloadError(string message, ARMObject<object> response) : base(message) {
			this.response = response;
			this.details = this.response["details"];
		}

		public string json() {
			return this.response.json();
		}
	}
}
