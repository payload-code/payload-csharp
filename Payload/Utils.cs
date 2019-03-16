using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using Payload.ARM;

namespace Payload {
	public class Utils {

		static public Type GetObjectClass(ARMObject<object> obj) {

			var typelist = from t in Assembly.GetExecutingAssembly().GetTypes()
				where t.IsClass && t.Namespace == "Payload"
				select t;

			Type class_found = null;

			foreach (var type in typelist) {
				if ( !type.BaseType.Name.Equals("ARMObject`1") )
					continue;

				var obj_template = (IARMObject)Activator.CreateInstance(type);

				if(!obj_template.GetSpec().sobject.Equals(obj["object"]))
					continue;

				if (obj_template.GetSpec().GetType().GetProperty("polymorphic") != null) {
					bool found = true;
					var poly = obj_template.GetSpec().polymorphic;

					PropertyInfo[] properties = poly.GetType().GetProperties();
					foreach (PropertyInfo pi in properties) {
						if (string.IsNullOrEmpty((string)obj[pi.Name])
						||  !string.Equals((string)obj[pi.Name], (string)pi.GetValue(poly,null)) ){
							found = false;
							break;
						}
					}
					if ( found ) {
						class_found = type;
						break;
					}
				} else if ( class_found == null )
					class_found = type;
			}

			return class_found;
		}

		static public void PopulateExpando( dynamic obj, dynamic data ) {
			if ( data == null ) return;

			var dictionary = (IDictionary<string, object>)obj;

			if ( data.GetType().BaseType.Name.Equals("ARMObject`1") ) {
				foreach (var key in data.Properties.Keys)
					dictionary.Add(key, data[key]);
			} else {
				var properties = data.GetType().GetProperties();

				foreach (var pi in properties)
					dictionary.Add(pi.Name, pi.GetValue(data, null));
			}
		}

		public static string ToQueryString(Dictionary<string, string> parameters) {
			var properties = from key in parameters.Keys
			                 select key + "=" + WebUtility.UrlEncode(parameters[key]);
			return String.Join("&", properties.ToArray());
		}

		public static string ToQueryString(object parameters) {
			var properties = from p in parameters.GetType().GetProperties()
			                 where p.GetValue(parameters, null) != null
			                 select p.Name + "=" + WebUtility.UrlEncode(p.GetValue(parameters, null).ToString());
			return String.Join("&", properties.ToArray());
		}

	}
}

