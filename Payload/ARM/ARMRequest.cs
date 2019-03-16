using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.IO;
using System.Reflection;
using System.Dynamic;
using System.Linq;
using Payload;


namespace Payload.ARM {

	public class ARMRequest {

		public dynamic Object;
		public Dictionary<string, string> _filters;
		public List<object> _attrs;
		public List<object> _group_by;

		private static JsonSerializerSettings jsonsettings = new JsonSerializerSettings{
			NullValueHandling = NullValueHandling.Ignore
		};

		public ARMRequest( Type type ) {
			this.Object = (IARMObject)Activator.CreateInstance(type);
			this._filters  = new Dictionary<string, string>();
			this._attrs    = new List<object>();
			this._group_by = new List<object>();
		}

		public dynamic request(string method, string id=null,
				object parameters=null, object json=null ) {
			var spec = this.Object.GetSpec();

			var endpoint = spec.GetType().GetProperty("endpoint") ?? "/" + spec.sobject + "s";
			if (!string.IsNullOrEmpty(id))
				endpoint += "/" + id;

			for ( int i = 0; i < this._attrs.Count; i++ )
				this._filters.Add( "attrs["+i.ToString()+"]", (string)this._attrs[i] );

			if ( this._filters.Count > 0 || parameters != null )
				endpoint += "?";

			if ( this._filters.Count > 0 ) {
				endpoint += Utils.ToQueryString(this._filters);
				if ( parameters != null )
					endpoint += "&";
			}

			if ( parameters != null )
				endpoint += Utils.ToQueryString(parameters);

			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(pl.api_url + endpoint);
			req.Method = method;

			string _auth = string.Concat(pl.api_key, ":");
			string _enc = Convert.ToBase64String(Encoding.ASCII.GetBytes(_auth));
			string _cred = string.Concat("Basic ", _enc);
			req.Headers.Add("Authorization", _cred);
			req.Accept = "application/json";

			if (json != null) {
				string post_data = JsonConvert.SerializeObject(
					json, Formatting.None, jsonsettings);
				var bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(post_data);

				req.ContentType = "application/json";
				req.ContentLength = bytes.Length;

				var writer = req.GetRequestStream();
				writer.Write(bytes, 0, bytes.Length);
			} else
				req.ContentLength = 0;

			var response = (HttpWebResponse)req.GetResponse();
			var reader = new StreamReader(response.GetResponseStream());
			var response_value = reader.ReadToEnd();

			var obj = JsonConvert.DeserializeObject<ARMObject<object>>(response_value);

			if (!string.IsNullOrEmpty(id) || (method == "POST" && !obj["object"].Equals("list") ) ) {

				dynamic result;
				if (!ARMObjectCache._cache.TryGetValue((string)obj["id"], out result)) {
					var type = Payload.Utils.GetObjectClass(obj);
					result = (IARMObject)Activator.CreateInstance(type);
				}

				result.Populate( obj );

				return result;
			} else {
				var return_list = new List<dynamic>();

				foreach( var i in (Newtonsoft.Json.Linq.JArray)obj["values"] ) {
					var item = i.ToObject<ARMObject<object>>();

					dynamic result;
					if (!ARMObjectCache._cache.TryGetValue((string)item["id"], out result)) {
						var type = Utils.GetObjectClass(item);
						result = (IARMObject)Activator.CreateInstance(type);
					}

					result.Populate( item );
					return_list.Add(result);
				}

				return return_list;
			}
		}

		public dynamic get( string id ) {
			if ( string.IsNullOrEmpty(id) )
				throw new ArgumentNullException("id cannot be empty");
			return this.request("GET", id: id);
		}

		public dynamic select(params dynamic[] attrs) {
			foreach( var attr in attrs )
				this._attrs.Add(attr.ToString());
			return this;
		}

		public dynamic create( dynamic data ) {

			dynamic obj = new ExpandoObject();
			if (data is IList<dynamic>) {

				var list = new List<dynamic>();
				foreach ( var item in data ) {

					dynamic row = new ExpandoObject();
					Utils.PopulateExpando( row, item );

					if ( this.Object.GetSpec().GetType().GetProperty("polymorphic") != null )
						Utils.PopulateExpando( row, this.Object.GetSpec().polymorphic );

					list.Add( row );
				}

				((IDictionary<string, object>)obj).Add("object", "list");
				obj.values = list;

			} else {

				Utils.PopulateExpando( obj, data );
				if ( this.Object.GetSpec().GetType().GetProperty("polymorphic") != null )
					Utils.PopulateExpando( obj, this.Object.GetSpec().polymorphic );

			}

			return this.request("POST", json:obj);
		}

		public dynamic update( dynamic updates ) {

			if (updates is IList<dynamic>) {
				for ( int i = 0; i < updates.Count; i++ ) {
					var upd = new ExpandoObject();
					((IDictionary<string, object>)upd).Add("id", updates[i][0]["id"]);
					Utils.PopulateExpando( upd, updates[i][1] );
					updates[i] = upd;
				}

				dynamic data = new ExpandoObject();
				((IDictionary<string, object>)data).Add("object", "list");
				data.values = updates;

				return this.request("PUT", json: data );
			}

			return this.request("PUT", parameters: new {mode="query"}, json: updates );
		}

		public dynamic delete( dynamic objects ) {

			if (objects is IList<dynamic>) {
				string id_query = String.Join("|", (from o in (List<dynamic>)objects select o.id).ToArray());

				return this.request("DELETE", parameters: new {mode="query", id=id_query} );
			}

			if ( this._filters.Count > 0 )
				return this.request("DELETE", parameters: new {mode="query"} );
			else
				throw new Exception("Invalid delete request");
		}

		public ARMRequest filter_by(params dynamic[] filters) {
			foreach( var filter in filters ) {
				if ( filter.GetType() == typeof(Filter) ) {
					this._filters.Add(filter.attr, filter.op + filter.val);
				} else {
					var properties = filter.GetType().GetProperties();

					foreach (var pi in properties)
						this._filters.Add(pi.Name, pi.GetValue(filter, null));
				}
			}

			return this;
		}

		public dynamic all() {
			return this.request("GET");
		}
	}
}

