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

		public static bool DEBUG = false;

		public dynamic Object = null;
		public Dictionary<string, dynamic> _filters;
		public List<object> _attrs;
		public List<object> _group_by;
		private pl.Session session;

		private static JsonSerializerSettings jsonsettings = new JsonSerializerSettings{
			NullValueHandling = NullValueHandling.Ignore
		};

		public ARMRequest(pl.Session session=null, Type type=null) {
			if ( type != null )
				this.Object = (IARMObject)Activator.CreateInstance(type);
			this._filters  = new Dictionary<string, dynamic>();
			this._attrs    = new List<object>();
			this._group_by = new List<object>();
			this.session = session != null ? session : pl.session;
		}

		public dynamic request(string method, string id=null,
				object parameters=null, object json=null ) {
			var spec = this.Object.GetSpec();

			var endpoint = spec.GetType().GetProperty("endpoint") != null ? spec.endpoint : "/" + spec.sobject + "s";
			if (!string.IsNullOrEmpty(id))
				endpoint += "/" + id;

			for ( int i = 0; i < this._attrs.Count; i++ )
				this._filters.Add( "fields["+i.ToString()+"]", (string)this._attrs[i] );

			if ( this._filters.Count > 0 || parameters != null )
				endpoint += "?";

			if ( this._filters.Count > 0 ) {
				endpoint += Utils.ToQueryString(this._filters);
				if ( parameters != null )
					endpoint += "&";
			}

			if ( parameters != null )
				endpoint += Utils.ToQueryString(parameters);

			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(this.session.api_url + endpoint);
			req.Method = method;

			string _auth = string.Concat(this.session.api_key, ":");
			string _enc = Convert.ToBase64String(Encoding.ASCII.GetBytes(_auth));
			string _cred = string.Concat("Basic ", _enc);
			req.Headers.Add("Authorization", _cred);
			req.Accept = "application/json";

			if (json != null) {

				var use_multipart = false;
				var data = Utils.JSONFlatten(json);
				foreach(var item in data) {
					if ( item.Value is FileStream ) {
						use_multipart = true;
						break;
					}
				}

				if ( use_multipart ) {
					string boundary = "----------" + DateTime.Now.Ticks.ToString("x");
					req.ContentType = "multipart/form-data; boundary=" + boundary;
					/// The first boundary
					byte[] boundarybytes = Encoding.UTF8.GetBytes("--" + boundary + "\r\n");
					/// the last boundary.
					byte[] trailer = Encoding.UTF8.GetBytes("--" + boundary + "--\r\n");
					/// the form data, properly formatted
					string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
					/// the form-data file upload, properly formatted
					string fileheaderTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\";\r\nContent-Type: {2}\r\n\r\n";


					int content_len = trailer.Length;
					foreach (string key in data.Keys)
					{
						content_len+=boundarybytes.Length+2;
						if ( data[key] is FileStream ) {
							var file = (FileStream)data[key];

							string contentType = MimeTypeMap.GetMimeType(Path.GetExtension(file.Name));
							content_len += string.Format(fileheaderTemplate, key, file.Name, contentType).Length;
							content_len += (int)file.Length;
						} else {
							content_len += string.Format(formdataTemplate, key, data[key]).Length;
						}
					}

					req.ContentLength = content_len;

					var writer = req.GetRequestStream();

					foreach (string key in data.Keys)
					{
						WriteToStream(writer, boundarybytes);

						if ( data[key] is FileStream ) {

							var file = (FileStream)data[key];

							string contentType = MimeTypeMap.GetMimeType(Path.GetExtension(file.Name));
							WriteToStream(writer, string.Format(fileheaderTemplate, key, file.Name, contentType));

							int CHUNK = 1024;
							int numBytesToRead = (int)file.Length;
							while (numBytesToRead > 0)
							{
								byte[] bytes = new byte[Math.Min(CHUNK, numBytesToRead)];
								int n = file.Read(bytes, 0, bytes.Length);

								if (n == 0)
									break;

								numBytesToRead -= n;
								WriteToStream(writer, bytes);
							}

						} else if ( data[key] is bool ) {
							WriteToStream(writer, string.Format(formdataTemplate, key,  ((bool)data[key])?"true":"false"));
						} else {
							WriteToStream(writer, string.Format(formdataTemplate, key, data[key]));
						}

						WriteToStream(writer, "\r\n");
					}

					WriteToStream(writer, trailer);


				} else {

					string post_data = JsonConvert.SerializeObject(
						json, Formatting.None, jsonsettings);

					var bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(post_data);

					if ( DEBUG ) {
						Console.WriteLine("-------------------REQ-------------------");
						Console.WriteLine(post_data);
					}

					req.ContentType = "application/json";
					req.ContentLength = bytes.Length;

					var writer = req.GetRequestStream();
					writer.Write(bytes, 0, bytes.Length);
				}
			}// else
			//	req.ContentLength = 0;

			HttpWebResponse response = null;
			try {
				response = (HttpWebResponse)req.GetResponse();
			} catch (WebException we) {
				response = we.Response as HttpWebResponse;
				if (response == null)
					throw;
			}

			string response_value = "";
			using (Stream dataStream = response.GetResponseStream())
			{
				StreamReader reader = new StreamReader(dataStream);
				response_value = reader.ReadToEnd();
			}

			response.Close();

			if ( DEBUG ) {
				Console.WriteLine("-------------------RESP------------------");
				Console.WriteLine(response_value);
			}

			var obj = JsonConvert.DeserializeObject<ARMObject<object>>(response_value);

			if (response.StatusCode == HttpStatusCode.OK) {

				if (!string.IsNullOrEmpty(id) || (method == "POST" && !obj["object"].Equals("list") ) ) {

					dynamic result = ARMObjectCache.GetOrCreate(obj, this.session);

					return result;
				} else {
					var return_list = new List<dynamic>();

					foreach( var i in (Newtonsoft.Json.Linq.JArray)obj["values"] ) {
						var item = i.ToObject<ARMObject<object>>();

						dynamic result = ARMObjectCache.GetOrCreate(item, this.session);

						return_list.Add(result);
					}

					return return_list;
				}
			} else {
				Type type = Utils.GetErrorClass(obj, (int)response.StatusCode);
				if ( type != null )
					throw (PayloadError)Activator.CreateInstance(type, (string)obj["error_description"], obj);
				throw new pl.UnknownResponse((string)obj["error_description"], obj);
			}
		}


		private int WriteToStream(Stream s, string txt) {
			return WriteToStream(s, Encoding.UTF8.GetBytes(txt));
		}

		private int WriteToStream(Stream s, byte[] bytes) {
			s.Write(bytes, 0, bytes.Length);
			return bytes.Length;
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

					_check_type( item );

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

				_check_type( data );

				if ( this.Object.GetSpec().GetType().GetProperty("polymorphic") != null )
					Utils.PopulateExpando( obj, this.Object.GetSpec().polymorphic );

			}

			return this.request("POST", json:obj);
		}

		public dynamic update( dynamic updates ) {

			if (updates is IList<dynamic>) {
				for ( int i = 0; i < updates.Count; i++ ) {

					_check_type( updates[i][0] );

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

		public dynamic delete( dynamic data=null ) {

			if (data is IList<dynamic>) {

				for ( int i = 0; i < data.Count; i++ )
					_check_type( data[i] );

				string id_query = String.Join("|",
					(from o in (List<dynamic>)data select o.id).ToArray());

				return this.request("DELETE", parameters: new {mode="query", id=id_query} );
			} else if (data != null) {
				if ( string.IsNullOrEmpty(data.id) )
					throw new ArgumentNullException("id cannot be empty");

				_check_type( data );

				return this.request("DELETE", id: data.id);
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

		public dynamic one() {
			var data =  this.request("GET", parameters: new {limit=1});
			if (data.Count == 1){
				return data[0];
			}
			else return null;
		}

		private void _check_type( dynamic obj ) {

			if ( Utils.IsSubclassOfRawGeneric(typeof(ARMObject<>), obj.GetType()) ) {
				if ( this.Object == null )
					this.Object = (IARMObject)Activator.CreateInstance(obj.GetType());
				else if ( this.Object.GetType() != obj.GetType() )
					throw new Exception("Bulk create requires all objects to be of the same type");
			} else if ( this.Object == null ) {
				throw new Exception("Bulk create requires ARMObject object types");
			}
		}
	}
}

