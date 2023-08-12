using System;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Payload.ARM
{

    public class Filter
    {
        public string attr;
        public string val;
        public string prefix;
        public string suffix;
        public Filter(Attr attr, string val, string prefix, string suffix = "")
        {
            this.attr = attr.ToString();
            this.val = val.ToString();
            this.prefix = prefix;
            this.suffix = suffix;
        }

        public override string ToString()
        {
            return string.Format("{0}{1}{2}{3}", attr, prefix, val, suffix);
        }
    }

    public class Attr : DynamicObject, IDynamicMetaObjectProvider
    {
        public string param;
        public Attr parent = null;
        public string key = null;
        public bool is_method = false;

        public Attr(string param, Attr parent = null, bool is_method = false)
        {
            this.param = param;
            if (parent != null && parent.param != null)
                this.parent = parent;
            this.is_method = is_method;

            if (this.parent == null)
                key = this.param;
            else
                key = String.Format("{0}[{1}]", this.parent.key, this.param);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = new Attr(binder.Name, this);
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            switch (binder.Name)
            {
                case "eq": result = new Filter(this, args[0].ToString(), ""); break;
                case "ne": result = new Filter(this, args[0].ToString(), "!"); break;
                case "gt": result = new Filter(this, args[0].ToString(), ">"); break;
                case "lt": result = new Filter(this, args[0].ToString(), "<"); break;
                case "ge": result = new Filter(this, args[0].ToString(), ">="); break;
                case "le": result = new Filter(this, args[0].ToString(), "<="); break;
                case "contains": result = new Filter(this, args[0].ToString(), "?*", "*"); break;
                default: result = new Attr(binder.Name, this, true); break;
            }
            return true;
        }

        public override string ToString()
        {
            if (is_method)
                return String.Format("{0}({1})", param, parent.key);
            return key;
        }
    }
}
