using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;
using System.Reflection;
using Payload;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Payload.ARM
{
    public class ARMObjectSpec
    {
        public string Object { get; set; } = "";
        public string Endpoint { get; set; }
        public pl.ARMObject Polymorphic { get; set; }
    }

    public static class ARMObjectCache
    {
        public static ConcurrentDictionary<string, object> _cache = new ConcurrentDictionary<string, object>();

        public static dynamic GetOrCreate(Type type, Dynamo data, Payload.Session session)
        {
            dynamic obj;
            if (type == data.GetType())
                obj = data;
            else
            {
                var armCls = Utils.GetObjectClass(data);
                if (armCls.IsSubclassOf(type))
                    obj = Activator.CreateInstance(armCls, data);
                else
                    obj = (dynamic)Activator.CreateInstance(type);
            }

            try
            {
                var id = (string)data["id"];

                if (
                    id != null
                    && _cache.TryGetValue(id, out dynamic cached)
                )
                {
                    if (data.GetType() == cached.GetType())
                        obj = cached;
                    else
                    {
                        obj.Populate(cached);
                    }
                }
            }
            catch (KeyNotFoundException)
            {
            }

            obj.session = session;
            obj.Populate(data);

            return obj;
        }

        public static T GetOrCreate<T>(T data, Payload.Session session) where T : ARMObjectBase<T>
        {
            return GetOrCreate(typeof(T), data, session);
        }
    }

    public class JSONObject : Dynamo
    {
        public string Json()
        {

            var obj = new ExpandoObject();
            Utils.PopulateExpando(obj, this);

            return JsonConvert.SerializeObject(
                    obj, Formatting.Indented);
        }
    }

    public interface IARMObject
    {
        ARMObjectSpec GetSpec();
    }

    public abstract class ARMObjectBase<T> : JSONObject, IARMObject where T : ARMObjectBase<T>
    {
        private static Dictionary<Type, pl.DefaultParams> _defaultParams = new Dictionary<Type, pl.DefaultParams>();
        public Payload.Session session = null;

        public static pl.DefaultParams DefaultParams
        {
            get
            {
                if (!_defaultParams.ContainsKey(typeof(T)))
                    _defaultParams[typeof(T)] = new pl.DefaultParams();
                return _defaultParams[typeof(T)];
            }
            set
            {
                _defaultParams[typeof(T)] = value;
            }
        }

        public ARMObjectBase()
        {
            Populate(new { });
        }

        public ARMObjectBase(object obj)
        {
            Populate(obj);
        }

        public virtual ARMObjectSpec GetSpec() => new ARMObjectSpec();

        private dynamic GetCachedObject()
        {
            if (
                caching
                && Properties.Keys.Contains("id")
                && ARMObjectCache._cache.ContainsKey((string)Properties["id"])
            )
            {
                return ARMObjectCache._cache[(string)Properties["id"]];
            }

            return null;
        }

        private bool caching = false;

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (!caching)
            {
                caching = true;

                var cached = GetCachedObject();
                if (cached != null)
                    return cached.TryGetMember(binder, out result);
            }

            caching = false;

            return base.TryGetMember(binder, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (!caching)
            {
                caching = true;

                var cached = GetCachedObject();
                if (cached != null)
                    cached.TrySetMember(binder, value);
            }

            caching = false;

            return base.TrySetMember(binder, value);
        }

        private object Convert(string key, dynamic val)
        {
            if (key.Equals("id"))
                ARMObjectCache._cache[val] = this;

            if (val == null)
                return null;

            if (val is Newtonsoft.Json.Linq.JArray jsonArr)
            {
                val = jsonArr.Select(i => Convert("", i.ToObject<Dynamo>())).ToArray();
            }

            if (val.GetType() == typeof(Newtonsoft.Json.Linq.JObject))
                val = ((Newtonsoft.Json.Linq.JObject)val).ToObject<Dynamo>();

            if (Utils.CheckIfAnonymousType(val.GetType()) && !(val is IList<dynamic>))
            {
                dynamic orig_val = val;
                val = new Dynamo();
                var properties = orig_val.GetType().GetProperties();

                foreach (var pi in properties)
                    ((Dynamo)val).Properties.Add(pi.Name, pi.GetValue(orig_val, null));
            }

            if (val is Dynamo dyn)
                if (dyn.Properties.Keys.Contains("object"))
                {
                    Type armCls = Utils.GetObjectClass(dyn);
                    var armObj = (Dynamo)Activator.CreateInstance(armCls, dyn);
                    val = ARMObjectCache.GetOrCreate(armCls, armObj, session);
                }

            if (val is IList<dynamic>)
            {
                var lst = new List<dynamic>();
                for (int i = 0; i < ((IList<dynamic>)val).Count; i++)
                {
                    lst.Add(Convert("", ((IList<dynamic>)val)[i]));
                    /*if (lst[i] == null) continue;

					if (lst[i].GetType().GetProperty("object") != null)
						lst[i] = ARMObjectCache.GetOrCreate(lst[i]);*/
                }

                return lst;
            }

            return val;
        }

        public T Populate(dynamic data)
        {
            void PopulateDynamo(Dynamo dyn)
            {
                List<string> keys = new List<string>();
                foreach (var key in dyn.Properties.Keys)
                    keys.Add(key);
                foreach (var key in keys)
                {
                    this[key] = Convert(key, dyn[key]);
                }
            }

            if (GetSpec().Polymorphic != null)
                PopulateDynamo(GetSpec().Polymorphic);

            if (data is Dynamo dataDynamo)
                PopulateDynamo(dataDynamo);
            else if (data is ExpandoObject)
            {
                var dict = (IDictionary<string, object>)data;
                foreach (var key in dict.Keys)
                    this[key] = Convert(key, dict[key]);
            }
            else
            {
                var properties = data.GetType().GetProperties();

                foreach (var pi in properties)
                    this[pi.Name] = Convert(pi.Name, pi.GetValue(data, null));
            }

            return (T)this;
        }

        public async Task<T> UpdateAsync(object update)
        {
            var body = new ExpandoObject();
            Utils.PopulateExpando(body, update);

            var req = new ARMRequest<T>(session);

            return await req.RequestAsync(RequestMethods.PUT, req.GetRoute((string)this["id"]), body);
        }

        public T Update(object update) => UpdateAsync(update).GetAwaiter().GetResult();

        public async Task<T> DeleteAsync()
        {
            var req = new ARMRequest<T>(session);

            return await req.RequestAsync(RequestMethods.DELETE, req.GetRoute((string)this["id"]));
        }

        public T Delete() => DeleteAsync().GetAwaiter().GetResult();

        public static async Task<T> GetAsync(string id, Payload.Session session = null)
        {
            var req = new ARMRequest<T>(session);

            return await req.RequestAsync(RequestMethods.GET, req.GetRoute(id));
        }

        public static T Get(string id, Payload.Session session = null) => GetAsync(id, session).GetAwaiter().GetResult();

        public static ARMRequest<T> FilterBy(params object[] list)
        {
            Payload.Session session = (Payload.Session)(list.Where(item => item is Payload.Session).FirstOrDefault() ?? pl.DefaultSession);
            List<dynamic> filters = list.Where(item => !(item is Payload.Session)).ToList();

            var req = new ARMRequest<T>(session);

            foreach (var filter in filters)
                req = req.FilterBy(filter);

            if (req.Spec.Polymorphic != null)
                req = req.FilterBy(req.Spec.Polymorphic);

            return req;
        }

        public static ARMRequest<T> Select(params object[] list)
        {
            Payload.Session session = (Payload.Session)(list.Where(item => item is Payload.Session).FirstOrDefault() ?? pl.DefaultSession);
            List<dynamic> attrs = list.Where(item => !(item is Payload.Session)).ToList();

            var req = new ARMRequest<T>(session);

            foreach (var attr in attrs)
                req = req.Select(attr);

            return req;
        }

        public static async Task<List<T>> CreateAllAsync(IEnumerable<dynamic> objects, Payload.Session session = null)
        {
            var lst = new List<dynamic>();
            foreach (dynamic obj in objects)
            {
                T newObj = (T)Activator.CreateInstance(typeof(T), obj);

                lst.Add(newObj);
            }

            return await new ARMRequest<T>(session).CreateAllAsync(lst);
        }

        public static List<T> CreateAll(IEnumerable<dynamic> objects, Payload.Session session = null) => CreateAllAsync(objects.ToArray(), session).GetAwaiter().GetResult();

        public static async Task<T> CreateAsync(dynamic attributes, Payload.Session session = null)
        {
            return await new ARMRequest<T>(session).CreateAsync(attributes);
        }

        public static T Create(dynamic obj = null, Payload.Session session = null) => CreateAsync(obj, session).GetAwaiter().GetResult();

        public static async Task<List<T>> UpdateAllAsync((T, object)[] updates, Payload.Session session = null)
        {
            return await new ARMRequest<T>(session).UpdateAllAsync(updates.ToArray());
        }

        public static List<T> UpdateAll((T, object)[] updates, Payload.Session session = null) => UpdateAllAsync(updates, session).GetAwaiter().GetResult();

        public static async Task<List<T>> DeleteAllAsync(List<T> objects, Payload.Session session = null)
        {
            return await new ARMRequest<T>(session).DeleteAllAsync(objects.ToArray());
        }

        public static List<T> DeleteAll(List<T> objects, Payload.Session session = null) => DeleteAllAsync(objects, session).GetAwaiter().GetResult();
    }
}
