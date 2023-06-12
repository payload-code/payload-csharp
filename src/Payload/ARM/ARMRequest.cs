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

    public class ARMRequest
    {

        public static bool DEBUG = false;

        public dynamic Object = null;
        public Dictionary<string, dynamic> _filters;
        public List<object> _attrs;
        public List<object> _group_by;
        private pl.Session session;

        private static JsonSerializerSettings jsonsettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        public ARMRequest(pl.Session session = null, Type type = null)
        {
            if (type != null)
                Object = (IARMObject)Activator.CreateInstance(type);
            _filters = new Dictionary<string, dynamic>();
            _attrs = new List<object>();
            _group_by = new List<object>();
            this.session = session != null ? session : pl.session;
        }

        public async Task<dynamic> RequestAsync(string method, string id = null,
                object parameters = null, object json = null)
        {
            var spec = this.Object.GetSpec();

            var endpoint = spec.GetType().GetProperty("endpoint") != null ? spec.endpoint : "/" + spec.sobject + "s";
            if (!string.IsNullOrEmpty(id))
                endpoint += "/" + id;

            for (int i = 0; i < this._attrs.Count; i++)
                this._filters.Add("fields[" + i.ToString() + "]", (string)this._attrs[i]);

            if (this._filters.Count > 0 || parameters != null)
                endpoint += "?";

            if (this._filters.Count > 0)
            {
                endpoint += Utils.ToQueryString(this._filters);
                if (parameters != null)
                    endpoint += "&";
            }

            if (parameters != null)
                endpoint += Utils.ToQueryString(parameters);

            using (var http = new HttpClient())
            {
                string _auth = string.Concat(this.session.api_key, ":");
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

                var response = await http.SendAsync(new HttpRequestMessage(new HttpMethod(method), this.session.api_url + endpoint) { Content = content });

                string response_value = await response.Content.ReadAsStringAsync();

                if (DEBUG)
                {
                    Console.WriteLine("-------------------RESP------------------");
                    Console.WriteLine(response_value);
                }

                var obj = JsonConvert.DeserializeObject<ARMObject<object>>(response_value);

                if (response.StatusCode == HttpStatusCode.OK)
                {

                    if (!string.IsNullOrEmpty(id) || (method == "POST" && !obj["object"].Equals("list")))
                    {

                        dynamic result = ARMObjectCache.GetOrCreate(obj, this.session);

                        return result;
                    }
                    else
                    {
                        var return_list = new List<dynamic>();

                        foreach (var i in (Newtonsoft.Json.Linq.JArray)obj["values"])
                        {
                            var item = i.ToObject<ARMObject<object>>();

                            dynamic result = ARMObjectCache.GetOrCreate(item, this.session);

                            return_list.Add(result);
                        }

                        return return_list;
                    }
                }
                else
                {
                    Type type = Utils.GetErrorClass(obj, (int)response.StatusCode);
                    if (type != null)
                        throw (PayloadError)Activator.CreateInstance(type, (string)obj["error_description"], obj);
                    throw new pl.UnknownResponse((string)obj["error_description"], obj);
                }
            }
        }

        public dynamic Request(string method, string id = null, object parameters = null, object json = null) =>
            RequestAsync(method, id, parameters, json).GetAwaiter().GetResult();
        
        [Obsolete]
        public dynamic request(string method, string id = null, object parameters = null, object json = null) =>
            Request(method, id, parameters, json);

        private int WriteToStream(Stream s, string txt)
        {
            return WriteToStream(s, Encoding.UTF8.GetBytes(txt));
        }

        private int WriteToStream(Stream s, byte[] bytes)
        {
            s.Write(bytes, 0, bytes.Length);
            return bytes.Length;
        }

        public async Task<dynamic> GetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("id cannot be empty");
            return await RequestAsync("GET", id: id);
        }

        public dynamic Get(string id) => GetAsync(id).GetAwaiter().GetResult();

        [Obsolete]
        public dynamic get(string id) => Get(id);

        public dynamic Select(params dynamic[] attrs)
        {
            foreach (var attr in attrs)
                _attrs.Add(attr.ToString());
            return this;
        }

        [Obsolete]
        public dynamic select(params dynamic[] attrs) => Select(attrs);

        public async Task<dynamic> CreateAsync(dynamic data)
        {

            dynamic obj = new ExpandoObject();
            if (data is IList<dynamic>)
            {
                var list = new List<dynamic>();
                foreach (var item in data)
                {

                    CheckType(item);

                    dynamic row = new ExpandoObject();
                    Utils.PopulateExpando(row, item);

                    if (this.Object.GetSpec().GetType().GetProperty("polymorphic") != null)
                        Utils.PopulateExpando(row, this.Object.GetSpec().polymorphic);

                    list.Add(row);
                }

                ((IDictionary<string, object>)obj).Add("object", "list");
                obj.values = list;

            }
            else
            {
                Utils.PopulateExpando(obj, data);

                CheckType(data);

                if (this.Object.GetSpec().GetType().GetProperty("polymorphic") != null)
                    Utils.PopulateExpando(obj, this.Object.GetSpec().polymorphic);

            }

            return await RequestAsync("POST", json: obj);
        }

        public dynamic Create(dynamic data) => CreateAsync(data).GetAwaiter().GetResult();

        [Obsolete]
        public dynamic create(dynamic data) => Create(data);

        public async Task<dynamic> UpdateAsync(dynamic updates)
        {

            if (updates is IList<dynamic>)
            {
                for (int i = 0; i < updates.Count; i++)
                {

                    CheckType(updates[i][0]);

                    var upd = new ExpandoObject();
                    ((IDictionary<string, object>)upd).Add("id", updates[i][0]["id"]);
                    Utils.PopulateExpando(upd, updates[i][1]);
                    updates[i] = upd;
                }

                dynamic data = new ExpandoObject();
                ((IDictionary<string, object>)data).Add("object", "list");
                data.values = updates;

                return await RequestAsync("PUT", json: data);
            }

            return await RequestAsync("PUT", parameters: new { mode = "query" }, json: updates);
        }

        public dynamic Update(dynamic updates) => UpdateAsync(updates).GetAwaiter().GetResult();

        [Obsolete]
        public dynamic update(dynamic updates) => Update(updates);

        public async Task<dynamic> DeleteAsync(dynamic data = null)
        {

            if (data is IList<dynamic>)
            {

                for (int i = 0; i < data.Count; i++)
                    CheckType(data[i]);

                string id_query = String.Join("|",
                    (from o in (List<dynamic>)data select o.id).ToArray());

                return await RequestAsync("DELETE", parameters: new { mode = "query", id = id_query });
            }
            else if (data != null)
            {
                if (string.IsNullOrEmpty(data.id))
                    throw new ArgumentNullException("id cannot be empty");

                CheckType(data);

                return await RequestAsync("DELETE", id: data.id);
            }

            if (_filters.Count > 0)
                return await RequestAsync("DELETE", parameters: new { mode = "query" });
            else
                throw new Exception("Invalid delete request");
        }

        public dynamic Delete(dynamic data = null) => DeleteAsync(data).GetAwaiter().GetResult();

        [Obsolete]
        public dynamic delete(dynamic data = null) => Delete(data);

        public ARMRequest FilterBy(params dynamic[] filters)
        {
            foreach (var filter in filters)
            {
                if (filter.GetType() == typeof(Filter))
                {
                    this._filters.Add(filter.attr, filter.op + filter.val);
                }
                else
                {
                    var properties = filter.GetType().GetProperties();

                    foreach (var pi in properties)
                        this._filters.Add(pi.Name, pi.GetValue(filter, null));
                }
            }

            return this;
        }

        [Obsolete]
        public ARMRequest filter_by(params dynamic[] filters) => FilterBy(filters);

        public async Task<dynamic> AllAsync() => await RequestAsync("GET");

        public dynamic All() => AllAsync().GetAwaiter().GetResult();

        [Obsolete]
        public dynamic all() => All();

        public async Task<dynamic> OneAsync()
        {
            var data = await RequestAsync("GET", parameters: new { limit = 1 });
            if (data.Count == 1)
            {
                return data[0];
            }
            else return null;
        }

        public dynamic One() => OneAsync().GetAwaiter().GetResult();

        [Obsolete]
        public dynamic one() => One();

        private void CheckType(dynamic obj)
        {

            if (Utils.IsSubclassOfRawGeneric(typeof(ARMObject<>), obj.GetType()))
            {
                if (this.Object == null)
                    this.Object = (IARMObject)Activator.CreateInstance(obj.GetType());
                else if (this.Object.GetType() != obj.GetType())
                    throw new Exception("Bulk create requires all objects to be of the same type");
            }
            else if (this.Object == null)
            {
                throw new Exception("Bulk create requires ARMObject object types");
            }
        }
    }
}

