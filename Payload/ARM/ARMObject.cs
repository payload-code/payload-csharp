using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;
using System.Reflection;
using Payload;

namespace Payload.ARM {
	interface IARMObject {
		dynamic GetSpec();
		dynamic Populate(dynamic obj);
	}

	public static class ARMObjectCache {
		public static Dictionary<string, dynamic> _cache = new Dictionary<string, dynamic>();

		public static dynamic GetOrCreate( dynamic obj ) {
			dynamic result;
			if (!ARMObjectCache._cache.TryGetValue((string)obj["id"], out result)) {
				var type = Payload.Utils.GetObjectClass(obj);
				result = (IARMObject)Activator.CreateInstance(type);
			}

			result.Populate( obj );
			return result;
		}
	}

	public class ARMObject<T>: Payload.Dynamo, IARMObject {

		public virtual dynamic GetSpec() {
			return new { sobject="" };
		}

		private dynamic Convert( string key, dynamic val ) {
			if ( key.Equals("id") )
				ARMObjectCache._cache[val] = this;

			if (val == null)
				return null;

			if(val.GetType()==typeof(Newtonsoft.Json.Linq.JArray))
				val = ((Newtonsoft.Json.Linq.JArray)val).ToObject<List<dynamic>>();

			if(val.GetType()==typeof(Newtonsoft.Json.Linq.JObject))
				val = ((Newtonsoft.Json.Linq.JObject)val).ToObject<Dynamo>();

			if(Utils.CheckIfAnonymousType(val.GetType()) && !(val is IList<dynamic>)) {
				dynamic orig_val = val;
				val = new Dynamo();
				var properties = orig_val.GetType().GetProperties();

				foreach (var pi in properties)
					((Dynamo)val).Properties.Add(pi.Name, pi.GetValue(orig_val, null));
			}

			if ( val is Dynamo )
				if (((Dynamo)val).Properties.Keys.Contains("object"))
					val = ARMObjectCache.GetOrCreate(val);

			if (val is IList<dynamic>) {
				var lst = ((IList<dynamic>)val);
				for( int i = 0; i < lst.Count; i++ ) {
					lst[i] = this.Convert("", lst[i]);
					/*if (lst[i] == null) continue;

					if (lst[i].GetType().GetProperty("object") != null)
						lst[i] = ARMObjectCache.GetOrCreate(lst[i]);*/
				}
			}

			return val;
		}

		public dynamic Populate( dynamic data ) {
			if ( GetSpec().GetType().GetProperty("polymorphic") != null ) {
				var poly = GetSpec().polymorphic;
				var properties = poly.GetType().GetProperties();

				foreach (var pi in properties)
					this[pi.Name] = this.Convert( pi.Name, pi.GetValue(poly, null) );
			}

			if ( data is Dynamo ) {
				foreach (var key in data.Properties.Keys) {
					this[key] = this.Convert( key, data[key] );
				}

			} else if ( data is ExpandoObject ) {
				var dict = (IDictionary<String, Object>) data;
				foreach( var key in dict.Keys )
					this[key] = this.Convert( key, dict[key] );
			} else {
				var properties = data.GetType().GetProperties();

				foreach (var pi in properties)
					this[pi.Name] = this.Convert( pi.Name, pi.GetValue(data, null) );
			}

			return this;
		}

		public string json() {
			return JsonConvert.SerializeObject(
					this, Formatting.None);
		}

		public void update( dynamic update ) {
			new ARMRequest(typeof(T)).request("PUT", id: (string)this["id"], json: update);
		}

		public void delete() {
			new ARMRequest(typeof(T)).request("DELETE", id: (string)this["id"]);
		}

		public static dynamic get( string id ) {
			return new ARMRequest(typeof(T)).get(id);
		}

		public static dynamic filter_by(params dynamic[] list) {
			var req = new ARMRequest(typeof(T));

			foreach ( var filters in list)
				req = req.filter_by(filters);

			if ( req.Object.GetSpec().GetType().GetProperty("polymorphic") != null )
				req = req.filter_by(req.Object.GetSpec().polymorphic);

			return req;
		}

		public static dynamic select(params dynamic[] list) {
			var req = new ARMRequest(typeof(T));

			foreach ( var attr in list)
				req = req.select(attr);

			if ( req.Object.GetSpec().GetType().GetProperty("polymorphic") != null )
				req = req.filter_by(req.Object.GetSpec().polymorphic);

			return req;
		}

		public static dynamic create(dynamic objects) {

			if (objects is IList<dynamic>) {
				var lst = new List<dynamic>();
				for ( int i = 0; i < objects.Length; i++ )
					lst.Add((T)((IARMObject)Activator.CreateInstance(typeof(T))).Populate(objects[i]));
				objects = lst;
			} else
				objects = (T)((IARMObject)Activator.CreateInstance(typeof(T))).Populate(objects);

			return new ARMRequest(typeof(T)).create(objects);
		}

		public static dynamic update_all( dynamic objects ) {
			return new ARMRequest(typeof(T)).update(objects);
		}

		public static dynamic delete_all( dynamic objects ) {
			return new ARMRequest(typeof(T)).delete(objects);
		}
	}
}
