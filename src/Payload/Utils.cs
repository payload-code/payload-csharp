using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Dynamic;
using System.Runtime.CompilerServices;
using Payload.ARM;

namespace Payload {
	public class Utils {

		static public Type GetObjectClass(ARMObject<object> obj) {
			return GetObjectClass((Dynamo)obj);
		}

		static public Type GetObjectClass(Dynamo obj) {

			var typelist = from t in Assembly.GetExecutingAssembly().GetTypes()
				where t.IsClass && t.Namespace == "Payload"
				select t;

			Type class_found = null;

			foreach (var type in typelist) {
				if ( !IsSubclassOfRawGeneric(typeof(ARMObject<>), type) )
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

		static public Type GetErrorClass(Dynamo obj, int code) {
			var typelist = from t in Assembly.GetExecutingAssembly().GetTypes()
				where t.IsClass && t.Namespace == "Payload"
				select t;

			Type class_found = null;

			foreach (var type in typelist) {
				if ( !IsSubclassOfRawGeneric(typeof(PayloadError), type) )
					continue;

				if ( !type.Name.Equals(obj["error_type"]) )
					continue;

				var obj_template = (IPayloadError)Activator.CreateInstance(type);

				if(obj_template.GetCode() != code)
					continue;

				class_found = type;
				break;
			}

			return class_found;
		}

		static public void PopulateExpando( dynamic obj, dynamic data ) {
			if ( data == null ) return;
			var dictionary = (IDictionary<string, object>)obj;

			if ( data is Dynamo ) {
				foreach (var key in data.Properties.Keys) {
					SetExpandoProperty( dictionary, key, data.Properties[key]);
				}
			} else if ( data is ExpandoObject ) {
				var dict = (IDictionary<String, Object>) data;
				foreach( var key in dict.Keys )
					SetExpandoProperty( dictionary, key, dict[key] );
			} else {
				var properties = data.GetType().GetProperties();

				foreach (var pi in properties)
					SetExpandoProperty( dictionary, pi.Name, pi.GetValue(data, null));
			}
		}

		static private void SetExpandoProperty( IDictionary<string, object> obj, string key, dynamic val ) {
			if (val == null) {
				obj[key] = val;
				return;
			}

			dynamic new_val;
			if (val is Dynamo) {
				new_val = new ExpandoObject();
				PopulateExpando( new_val, val );
				val = new_val;
			}

			if (val is IList<dynamic>) {
				new_val = new List<dynamic>();
				for( int i = 0; i < val.Length; i++ ) {
					if (val[i] == null) {
						new_val.Add(val[i]);
					} else if (val[i] is Dynamo) {
						new_val.Add(new ExpandoObject());
						PopulateExpando( new_val[i], val[i] );
					} else {
						new_val.Add(val[i]);
					}
				}
				val = new_val;
			}

			obj[key] = val;
		}

		public static string ToQueryString(Dictionary<string, dynamic> parameters) {
			var properties = from key in parameters.Keys
			                 select key + "=" + WebUtility.UrlEncode(string.Empty + parameters[key]);
			return String.Join("&", properties.ToArray());
		}

		public static string ToQueryString(object parameters) {
			var properties = from p in parameters.GetType().GetProperties()
			                 where p.GetValue(parameters, null) != null
			                 select p.Name + "=" + WebUtility.UrlEncode(p.GetValue(parameters, null).ToString());
			return String.Join("&", properties.ToArray());
		}

		public static bool IsSubclassOfRawGeneric(Type generic, Type toCheck) {
			while (toCheck != null && toCheck != typeof(object)) {
				var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
				if (generic == cur) {
					return true;
				}
				toCheck = toCheck.BaseType;
			}
			return false;
		}

		public static bool CheckIfAnonymousType(Type type) {
			if (type == null)
				throw new ArgumentNullException("type");

			// HACK: The only way to detect anonymous types right now.
			return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
				&& type.IsGenericType && type.Name.Contains("AnonymousType")
				&& (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
				&& (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
		}

	}
}

