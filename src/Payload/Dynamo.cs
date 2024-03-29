using System;
using System.Linq;
using System.Dynamic;
using System.Collections.Generic;
using System.Reflection;

namespace Payload
{

    public class Dynamo : DynamicObject, IDynamicMetaObjectProvider
    {
        public Dictionary<string, object> Properties = new Dictionary<string, object>();

        public dynamic Data => this;

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (Properties.Keys.Contains(binder.Name))
            {
                result = Properties[binder.Name];
                return true;
            }

            return GetMember(binder.Name, MemberTypes.Property, out result);
        }

        public bool GetMember(string name, MemberTypes type, out object result)
        {
            var members = GetType().GetMember(name,
                BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.Instance);

            if (members == null || members.Length == 0 || members[0].MemberType != type)
            {
                result = null;
                return false;
            }

            result = ((PropertyInfo)members[0]).GetValue(this, null);
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {

            if (SetMember(binder.Name, MemberTypes.Property, value))
                return true;

            Properties[binder.Name] = value;
            return true;
        }

        public bool SetMember(string name, MemberTypes type, object value)
        {
            var members = GetType().GetMember(name,
                BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.Instance);

            if (members == null || members.Length == 0 || members[0].MemberType != type)
                return false;

            ((PropertyInfo)members[0]).SetValue(this, value, null);
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var members = GetType().GetMember(binder.Name,
                BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance);

            if (members == null || members.Length == 0)
            {
                result = null;
                return false;
            }

            result = ((MethodInfo)members[0]).Invoke(this, args);
            return true;
        }

        public bool HasObject(string key) => Properties.ContainsKey(key);

        public Dynamo GetObject(string key) => (Dynamo)this[key];

        public T GetProperty<T>(string key) => (T)this[key];

        public void SetProperty(string key, dynamic val) => this[key] = val;

        public object this[string key]
        {
            get
            {
                if (Properties.ContainsKey(key))
                    return Properties[key];

                object result = null;
                if (GetMember(key, MemberTypes.Property, out result))
                    return result;

                throw new KeyNotFoundException();
            }

            set
            {
                if (!SetMember(key, MemberTypes.Property, value))
                    Properties[key] = value;
            }
        }
    }
}

