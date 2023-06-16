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

namespace Payload.ARM
{
    public class RequestArgs
    {
        public string Method { get; set; }
        public object Parameters { get; set; }
        public ExpandoObject Body { get; set; }
    }

    public class ARMRequest<T> where T : ARMObjectBase<T>
    {

        public static bool DEBUG = false;

        public ARMObjectSpec Spec { get; set; }
        public Dictionary<string, dynamic> _filters;
        public List<dynamic> _attrs;
        public List<object> _group_by;
        private pl.Session session;

        private static JsonSerializerSettings jsonsettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        public ARMRequest(pl.Session session = null)
        {
            var obj = (T)Activator.CreateInstance(typeof(T));
            Spec = obj.GetSpec();
            _filters = new Dictionary<string, dynamic>();
            _attrs = new List<dynamic>();
            _group_by = new List<object>();
            this.session = session != null ? session : pl.DefaultSession;
        }

        private async Task<JSONObject> ExecuteRequestAsync(RequestArgs args, string id = null)
        {
            var method = args.Method;
            var parameters = args.Parameters;
            var json = args.Body;

            var endpoint = Spec.Endpoint != null ? Spec.Endpoint : "/" + Spec.Object + "s";
            if (!string.IsNullOrEmpty(id))
                endpoint += "/" + id;

            for (int i = 0; i < _attrs.Count; i++)
                _filters.Add("fields[" + i.ToString() + "]", (string)_attrs[i]);

            if (_filters.Count > 0 || parameters != null)
                endpoint += "?";

            if (_filters.Count > 0)
            {
                endpoint += Utils.ToQueryString(_filters);
                if (parameters != null)
                    endpoint += "&";
            }

            if (parameters != null)
                endpoint += Utils.ToQueryString(parameters);

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
                if (json != null)
                {
                    var use_multipart = false;
                    var data = Utils.JSONFlatten(json);
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
                            json, Formatting.None, jsonsettings);

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

                var response = await http.SendAsync(new HttpRequestMessage(new HttpMethod(method), session.ApiUrl + endpoint) { Content = content });

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

        public async Task<List<T>> RequestAllAsync(RequestArgs args)
        {
            var obj = await ExecuteRequestAsync(args);

            var return_list = new List<T>();

            foreach (var i in (Newtonsoft.Json.Linq.JArray)obj["values"])
            {
                var item = i.ToObject<T>();

                T result = ARMObjectCache.GetOrCreate(item, session);

                return_list.Add(result);
            }

            return return_list;
        }

        public async Task<T> RequestAsync(RequestArgs args, string id = null)
        {
            var obj = await ExecuteRequestAsync(args, id);

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
            return await RequestAsync(new RequestArgs() { Method = "GET" }, id);
        }

        public T Get(string id) => GetAsync(id).GetAwaiter().GetResult();

        public ARMRequest<T> Select(params dynamic[] attrs)
        {
            foreach (var attr in attrs)
                _attrs.Add(attr.ToString());
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


            return await RequestAllAsync(new RequestArgs() { Method = "POST", Body = body });
        }

        public List<T> CreateAll(IEnumerable<object> objects) => CreateAllAsync(objects).GetAwaiter().GetResult();

        public async Task<T> CreateAsync(object data)
        {
            var body = new ExpandoObject();

            Utils.PopulateExpando(body, data);

            if (Spec.Polymorphic != null)
                Utils.PopulateExpando(body, Spec.Polymorphic);

            return await RequestAsync(new RequestArgs() { Method = "POST", Body = body });
        }

        public T Create(object data) => CreateAsync(data).GetAwaiter().GetResult();

        public async Task<List<T>> UpdateAllAsync(object[] updates)
        {
            var updateBody = new List<ExpandoObject>();
            for (int i = 0; i < updates.Count(); i++)
            {
                var upd = new ExpandoObject();

                if (((object[])updates[i])[0] is T obj)
                {
                    ((IDictionary<string, object>)upd).Add("id", (string)obj["id"]);
                    Utils.PopulateExpando(upd, ((object[])updates[i])[1]);
                }
                else
                    new Exception($"Object at index {i} is not the ARMObject class {typeof(T).Name}");

                updateBody[i] = upd;
            }

            var body = new ExpandoObject();

            ((IDictionary<string, object>)body).Add("object", "list");
            ((IDictionary<string, object>)body).Add("values", updates);

            return await RequestAllAsync(new RequestArgs() { Method = "PUT", Body = body });
        }

        public List<T> UpdateAll(object[] updates) => UpdateAllAsync(updates).GetAwaiter().GetResult();

        public async Task<T> UpdateAsync(T update)
        {
            string id = (string)update["id"];

            var body = new ExpandoObject();
            Utils.PopulateExpando(body, update);

            return await RequestAsync(new RequestArgs() { Method = "PUT", Parameters = new { mode = "query" }, Body = body }, id);
        }

        public T Update(T update) => UpdateAsync(update).GetAwaiter().GetResult();

        public async Task<List<T>> DeleteAllAsync(IEnumerable<T> deletes)
        {
            if (deletes.Any())
            {
                string id_query = string.Join("|", deletes.Select(d => d.Data.id));

                return await RequestAllAsync(new RequestArgs() { Method = "DELETE", Parameters = new { mode = "query", id = id_query } });
            }

            if (_filters.Count > 0)
                return await RequestAllAsync(new RequestArgs() { Method = "DELETE", Parameters = new { mode = "query" } });
            else
                throw new Exception("Must set at least one filter to delete using query mode");
        }

        public List<T> DeleteAll(IEnumerable<T> obj) => DeleteAllAsync(obj).GetAwaiter().GetResult();

        public async Task<T> DeleteAsync(T data)
        {
            string id = (string)data["id"];
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("id cannot be empty");

            return await RequestAsync(new RequestArgs() { Method = "DELETE" }, id);
        }

        public T Delete(T obj) => DeleteAsync(obj).GetAwaiter().GetResult();

        public ARMRequest<T> FilterBy(params dynamic[] filters)
        {
            foreach (var filter in filters)
            {
                if (filter.GetType() == typeof(Filter))
                {
                    _filters.Add(filter.attr, filter.op + filter.val);
                }
                else if (filter is Dynamo dynamo)
                {
                    foreach (var pi in dynamo.Properties)
                        _filters.Add(pi.Key, pi.Value);
                }
                else
                {
                    var properties = filter.GetType().GetProperties();

                    foreach (var pi in properties)
                        _filters.Add(pi.Name, pi.GetValue(filter, null));
                }
            }

            return this;
        }

        public ARMRequest<T> Offset(int offset)
        {
            _filters["offset"] = offset;
            return this;
        }

        public ARMRequest<T> Limit(int limit)
        {
            _filters["limit"] = limit;
            return this;
        }

        public ARMRequest<T> Range(int offset, int end)
        {
            Offset(offset);
            Limit(end - offset);

            return this;
        }

        public async Task<List<T>> AllAsync() => await RequestAllAsync(new RequestArgs() { Method = "GET" });

        public List<T> All() => AllAsync().GetAwaiter().GetResult();

        public async Task<T> OneAsync()
        {
            var data = await RequestAllAsync(new RequestArgs() { Method = "GET", Parameters = new { limit = 1 } });
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

