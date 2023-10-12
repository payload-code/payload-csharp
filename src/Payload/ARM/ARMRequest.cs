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
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections;

namespace Payload.ARM
{
    public enum RequestMethods { GET, POST, PUT, DELETE, PATCH }

    public class ARMRequest<T> where T : ARMObjectBase<T>
    {

        public static bool DEBUG = false;

        public ARMObjectSpec Spec { get; set; }
        public List<(string, object)> _filters;
        public List<string> _fields;
        public List<string> _group_by;
        public List<string> _order_by;
        private pl.Session session;

        private static JsonSerializerSettings jsonsettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        private void Init(pl.Session session = null)
        {
            var obj = (T)Activator.CreateInstance(typeof(T));
            Spec = obj.GetSpec();
            _filters = new List<(string, object)>();
            _fields = new List<string>();
            _group_by = new List<string>();
            _order_by = new List<string>();
            this.session = session != null ? session : pl.DefaultSession;

            _fields.AddRange(ARMObjectBase<T>.DefaultParams.Fields.Select(f => f.ToString()));
        }

        public ARMRequest()
        {
            Init();
        }

        public ARMRequest(pl.Session session)
        {
            Init(session);
        }

        public string GetRoute(string id = null)
        {
            var route = Spec.Endpoint != null ? Spec.Endpoint : "/" + Spec.Object + "s";
            if (!string.IsNullOrEmpty(id))
                route += "/" + id;

            for (int i = 0; i < _fields.Count; i++)
                _filters.Add(("fields[" + i.ToString() + "]", _fields[i]));

            for (int i = 0; i < _group_by.Count; i++)
                _filters.Add(("group_by[" + i.ToString() + "]", _group_by[i]));

            for (int i = 0; i < _order_by.Count; i++)
                _filters.Add(("order_by[" + i.ToString() + "]", _order_by[i]));

            if (_filters.Count > 0 || Spec.Polymorphic != null)
                route += "?";

            if (_filters.Count > 0)
            {
                route += Utils.ToQueryString(_filters);
            }

            if (Spec.Polymorphic != null)
            {
                route += "&";

                var polyObj = new ExpandoObject();
                Utils.PopulateExpando(polyObj, Spec.Polymorphic);
                route += Utils.ToQueryString(polyObj.Select(p => (p.Key, p.Value)));
            }

            return route;
        }

        public virtual async Task<JSONObject> ExecuteRequestAsync(RequestMethods method, string route, ExpandoObject body = null)
        {
            using (var http = new HttpClient())
            {
                string _auth = string.Concat(session.ApiKey, ":");
                string _enc = Convert.ToBase64String(Encoding.ASCII.GetBytes(_auth));
                string _cred = string.Concat("Basic ", _enc);
                http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", _enc);
                http.DefaultRequestHeaders.Accept.Add(
                   new MediaTypeWithQualityHeaderValue("application/json"));

                HttpContent content;
                if (body != null)
                {
                    var use_multipart = false;
                    var data = Utils.JSONFlatten(body);
                    foreach (var item in data)
                    {
                        if (item.Value is FileStream)
                        {
                            use_multipart = true;
                            break;
                        }
                    }

                    if (use_multipart)
                    {
                        var multipart = new MultipartFormDataContent();
                        foreach (string key in data.Keys)
                        {
                            if (data[key] is FileStream)
                            {
                                var file = (FileStream)data[key];
                                var streamContent = new StreamContent(file);
                                streamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                                {
                                    Name = key,
                                    FileName = file.Name
                                };
                                multipart.Add(streamContent, key, file.Name);
                            }
                            else if (data[key] is bool)
                            {
                                multipart.Add(new StringContent(((bool)data[key]) ? "true" : "false"), key);
                            }
                            else
                            {
                                try
                                {
                                    multipart.Add(new StringContent((string)data[key]), key);
                                }
                                catch (InvalidCastException)
                                {
                                    multipart.Add(new StringContent(data[key].ToString()), key);
                                }
                            }
                        }
                        content = multipart;
                    }
                    else
                    {
                        string post_data = JsonConvert.SerializeObject(
                            body, Formatting.None, jsonsettings);

                        var bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(post_data);

                        if (DEBUG)
                        {
                            Console.WriteLine("-------------------REQ-------------------");
                            Console.WriteLine(post_data);
                        }

                        content = new ByteArrayContent(bytes);
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    }
                }
                else
                {
                    content = null;
                }

                var response = await http.SendAsync(new HttpRequestMessage(new HttpMethod(method.ToString()), session.ApiUrl + route) { Content = content });

                string response_value = await response.Content.ReadAsStringAsync();

                if (DEBUG)
                {
                    Console.WriteLine("-------------------RESP------------------");
                    Console.WriteLine(response_value);
                }

                var jsonObj = JsonConvert.DeserializeObject<JSONObject>(response_value);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return jsonObj;
                }
                else
                {
                    Type type = Utils.GetErrorClass(jsonObj, (int)response.StatusCode);
                    if (type != null)
                        throw (PayloadError)Activator.CreateInstance(type, (string)jsonObj["error_description"], jsonObj);
                    throw new pl.UnknownResponse((string)jsonObj["error_description"], jsonObj);
                }
            }
        }

        public async Task<List<T>> RequestAllAsync(RequestMethods method, string route, ExpandoObject body = null)
        {
            var obj = await ExecuteRequestAsync(method, route, body);

            var return_list = new List<T>();

            foreach (var i in (Newtonsoft.Json.Linq.JArray)obj["values"])
            {
                var item = i.ToObject<T>();

                T result = ARMObjectCache.GetOrCreate(item, session);

                return_list.Add(result);
            }

            return return_list;
        }

        public async Task<T> RequestAsync(RequestMethods method, string route, ExpandoObject body = null)
        {
            var obj = await ExecuteRequestAsync(method, route, body);

            T result = (T)Activator.CreateInstance(typeof(T), obj);

            result = ARMObjectCache.GetOrCreate(result, session);

            return result;
        }

        private int WriteToStream(Stream s, string txt)
        {
            return WriteToStream(s, Encoding.UTF8.GetBytes(txt));
        }

        private int WriteToStream(Stream s, byte[] bytes)
        {
            s.Write(bytes, 0, bytes.Length);
            return bytes.Length;
        }

        public async Task<T> GetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("id cannot be empty");
            return await RequestAsync(RequestMethods.GET, GetRoute(id));
        }

        public T Get(string id) => GetAsync(id).GetAwaiter().GetResult();

        public ARMRequest<T> Select(params object[] fields)
        {
            _fields.AddRange(fields.Select(v => v.ToString()));

            return this;
        }

        public async Task<List<T>> CreateAllAsync(IEnumerable<object> objects)
        {
            var body = new ExpandoObject();

            var list = new List<ExpandoObject>();
            foreach (var item in objects)
            {
                var row = new ExpandoObject();
                Utils.PopulateExpando(row, item);

                if (Spec.Polymorphic != null)
                    Utils.PopulateExpando(row, Spec.Polymorphic);

                list.Add(row);
            }

            ((IDictionary<string, object>)body).Add("object", "list");
            ((IDictionary<string, object>)body).Add("values", list);

            return await RequestAllAsync(RequestMethods.POST, GetRoute(), body);
        }

        public List<T> CreateAll(IEnumerable<object> objects) => CreateAllAsync(objects).GetAwaiter().GetResult();

        public async Task<T> CreateAsync(dynamic attributes)
        {
            object obj = (T)Activator.CreateInstance(typeof(T), attributes);
            var body = new ExpandoObject();

            Utils.PopulateExpando(body, obj ?? new { });

            if (Spec.Polymorphic != null)
                Utils.PopulateExpando(body, Spec.Polymorphic);

            return await RequestAsync(RequestMethods.POST, GetRoute(), body);
        }

        public T Create(object data = null) => CreateAsync(data).GetAwaiter().GetResult();

        public async Task<List<T>> UpdateAllAsync((T, object)[] updates)
        {
            var values = new List<ExpandoObject>();
            foreach (var update in updates)
            {
                var updateBody = new ExpandoObject();

                ((IDictionary<string, object>)updateBody).Add("id", (string)update.Item1["id"]);
                Utils.PopulateExpando(updateBody, update.Item2);

                values.Add(updateBody);
            }

            var body = new ExpandoObject();

            ((IDictionary<string, object>)body).Add("object", "list");
            ((IDictionary<string, object>)body).Add("values", values);

            return await RequestAllAsync(RequestMethods.PUT, GetRoute(), body);
        }

        public List<T> UpdateAll(params (T, object)[] updates) => UpdateAllAsync(updates).GetAwaiter().GetResult();

        public async Task<List<T>> QueryUpdateAsync(object update)
        {
            var body = new ExpandoObject();
            Utils.PopulateExpando(body, update);

            _filters.Add(("mode", "query"));

            return await RequestAllAsync(RequestMethods.PUT, GetRoute(), body);
        }

        public List<T> QueryUpdate(object update) => QueryUpdateAsync(update).GetAwaiter().GetResult();

        public async Task<List<T>> DeleteAllAsync(params T[] deletes)
        {
            if (deletes.Any())
            {
                _filters.Add(("mode", "query"));

                string id_query = string.Join("|", deletes.Select(d => d.Data.id));
                _filters.Add(("id", id_query));

                return await RequestAllAsync(RequestMethods.DELETE, GetRoute());
            }

            return new List<T>();
        }

        public async Task<List<T>> DeleteAllAsync(List<T> deletes) => await DeleteAllAsync(deletes.ToArray());

        public List<T> DeleteAll(params T[] obj) => DeleteAllAsync(obj).GetAwaiter().GetResult();

        public List<T> DeleteAll(List<T> obj) => DeleteAll(obj.ToList());

        public async Task<List<T>> QueryDeleteAsync()
        {
            if (_filters.Count > 0)
            {
                _filters.Add(("mode", "query"));
                return await RequestAllAsync(RequestMethods.DELETE, GetRoute());
            }
            else
                throw new Exception("Must set at least one filter to delete using query mode");
        }

        public List<T> QueryDelete() => QueryDeleteAsync().GetAwaiter().GetResult();

        public async Task<T> DeleteAsync(T data)
        {
            string id = (string)data["id"];
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("id cannot be empty");

            return await RequestAsync(RequestMethods.DELETE, GetRoute(id));
        }

        public T Delete(T obj) => DeleteAsync(obj).GetAwaiter().GetResult();

        public ARMRequest<T> FilterBy(object filters)
        {
            if (!(filters is IEnumerable))
                filters = new[] { filters };

            foreach (var filter in (IEnumerable<object>)filters)
            {
                if (filter is Filter f)
                {
                    _filters.Add((f.attr, f.prefix + f.val));
                }
                else if (filter is Dynamo dynamo)
                {
                    foreach (var pi in dynamo.Properties)
                        _filters.Add((pi.Key, pi.Value));
                }
                else
                {
                    var properties = filter.GetType().GetProperties();

                    foreach (var pi in properties)
                        _filters.Add((pi.Name, pi.GetValue(filter, null)));
                }
            }

            return this;
        }

        public ARMRequest<T> GroupBy(object groupBy)
        {
            var groupBys = new List<object>();
            if (!(groupBy is IEnumerable))
                groupBys = new List<object>() { groupBy };
            else
                groupBys = ((IEnumerable<object>)groupBy).ToList();

            _group_by.AddRange(groupBys.Select(gb => gb.ToString()));

            return this;
        }

        public ARMRequest<T> OrderBy(object[] orderBys)
        {
            _order_by.AddRange(orderBys.Select(ob => ob.ToString()));

            return this;
        }

        public ARMRequest<T> Offset(int offset)
        {
            _filters.Add(("offset", offset));
            return this;
        }

        public ARMRequest<T> Limit(int limit)
        {
            _filters.Add(("limit", limit));
            return this;
        }

        public ARMRequest<T> Range(int offset, int end)
        {
            Offset(offset);
            Limit(end - offset);

            return this;
        }

        public async Task<List<T>> AllAsync() => await RequestAllAsync(RequestMethods.GET, GetRoute());

        public List<T> All() => AllAsync().GetAwaiter().GetResult();

        public async Task<T> OneAsync()
        {
            _filters.Add(("limit", 1));

            var data = await RequestAllAsync(RequestMethods.GET, GetRoute());
            if (data.Count() == 1)
            {
                return data.First();
            }
            else
                return null;
        }

        public T One() => OneAsync().GetAwaiter().GetResult();
    }
}

