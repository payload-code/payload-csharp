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

		public static dynamic GetOrCreate( dynamic obj, pl.Session session ) {
			dynamic result;
			try {
				var id = obj["id"];

				if (!ARMObjectCache._cache.TryGetValue((string)id, out result)) {
					var type = Payload.Utils.GetObjectClass(obj);
					if ( type == null )
						return obj;
					result = (IARMObject)Activator.CreateInstance(type);
				}
			} catch ( KeyNotFoundException exc ) {
				return obj;
			}
			result.session = session;
			result.Populate( obj );
			return result;
		}
	}

	public class ARMObject<T>: Payload.Dynamo, IARMObject {
		public pl.Session session = null;

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
					val = ARMObjectCache.GetOrCreate(val, this.session);

			if (val is IList<dynamic>) {
				var lst = new List<dynamic>();
				for( int i = 0; i < ((IList<dynamic>)val).Count; i++ ) {
					lst.Add(this.Convert("", ((IList<dynamic>)val)[i]));
					/*if (lst[i] == null) continue;

					if (lst[i].GetType().GetProperty("object") != null)
						lst[i] = ARMObjectCache.GetOrCreate(lst[i]);*/
				}

				val = lst;
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

			dynamic obj = new ExpandoObject();
			Utils.PopulateExpando( obj, this );

			return JsonConvert.SerializeObject(
					obj, Formatting.Indented);
		}

		public void update( dynamic update ) {
			new ARMRequest(this.session, typeof(T)).request("PUT", id: (string)this["id"], json: update);
		}

		public void delete() {
			new ARMRequest(this.session, typeof(T)).request("DELETE", id: (string)this["id"]);
		}

		public static dynamic get( string id, pl.Session session=null ) {
			return new ARMRequest(session, typeof(T)).get(id);
		}

		public static dynamic filter_by(params dynamic[] list) {
			pl.Session session = list.Where(item => item is pl.Session).FirstOrDefault();
			List<dynamic> filters = list.Where(item => !(item is pl.Session)).ToList();

			var req = new ARMRequest(session, typeof(T));

			foreach (var filter in filters)
				req = req.filter_by(filter);

			if ( req.Object.GetSpec().GetType().GetProperty("polymorphic") != null )
				req = req.filter_by(req.Object.GetSpec().polymorphic);

			return req;
		}

		public static dynamic select(params dynamic[] list) {
			pl.Session session = list.Where(item => item is pl.Session).FirstOrDefault();
			List<dynamic> attrs = list.Where(item => !(item is pl.Session)).ToList();

			var req = new ARMRequest(session, typeof(T));

			foreach ( var attr in attrs)
				req = req.select(attr);

			if ( req.Object.GetSpec().GetType().GetProperty("polymorphic") != null )
				req = req.filter_by(req.Object.GetSpec().polymorphic);

			return req;
		}

		public static dynamic create(dynamic objects, pl.Session session=null) {
			if (objects is IList<dynamic>) {
				var lst = new List<dynamic>();
				for ( int i = 0; i < ((IList<dynamic>)objects).Count; i++ )
					lst.Add((T)((IARMObject)Activator.CreateInstance(typeof(T))).Populate(objects[i]));
				objects = lst;
			} else
				objects = (T)((IARMObject)Activator.CreateInstance(typeof(T))).Populate(objects);

			return new ARMRequest(session, typeof(T)).create(objects);
		}

		public static dynamic update_all( dynamic objects, pl.Session session=null ) {
			return new ARMRequest(session, typeof(T)).update(objects);
		}

		public static dynamic delete_all( dynamic objects, pl.Session session=null ) {
			return new ARMRequest(session, typeof(T)).delete(objects);
		}
	}
}
