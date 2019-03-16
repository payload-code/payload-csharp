using System;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;
using System.Reflection;
using Payload;

namespace Payload.ARM {
	interface IARMObject {
		dynamic GetSpec();
	}

	public static class ARMObjectCache {
		public static Dictionary<string, dynamic> _cache = new Dictionary<string, dynamic>();
	}

	public class ARMObject<T>: Payload.Dynamo, IARMObject {

		public dynamic GetSpec() {
			return new { sobject="" };
		}

		public void Populate( dynamic data ) {
			if ( data.GetType().Name.Equals("ARMObject`1")
			|| ( data.GetType().BaseType.Name.Equals("ARMObject`1") )) {
				foreach (var key in data.Properties.Keys) {
					if ( key.Equals("id") )
						ARMObjectCache._cache[data[key]] = this;

					this[key] = data[key];
				}
			} else {
				var properties = data.GetType().GetProperties();

				foreach (var pi in properties) {
					if ( pi.Name.Equals("id") )
						ARMObjectCache._cache[ pi.GetValue(data, null) ] = this;

					this[pi.Name] = pi.GetValue(data, null);
				}
			}
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
