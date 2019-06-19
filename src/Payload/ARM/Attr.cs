using System;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Payload.ARM {

	public class Filter {
		public string attr;
		public string val;
		public string op;
		public Filter( Attr attr, string val, string op ) {
			this.attr = attr.ToString();
			this.val = val.ToString();
			this.op = op;
		}

		public override string ToString() {
			return String.Format("{0}{1}{2}", this.attr, this.op, this.val );
		}
	}

	public class Attr : DynamicObject, IDynamicMetaObjectProvider {
		public string param;
		public Attr parent = null;
		public string key = null;
		public bool is_method = false;

		public Attr(string param, Attr parent=null, bool is_method=false) {
			this.param = param;
			if ( parent != null && parent.param != null )
				this.parent = parent;
			this.is_method = is_method;

			if ( this.parent == null )
				this.key = this.param;
			else
				this.key = String.Format("{0}[{1}]", this.parent.key, this.param);
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result) {
			result = new Attr(binder.Name,this);
			return true;
		}

		public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result) {
			result = new Attr(binder.Name, this, true);
			return true;
		}

		public override string ToString() {
			if ( this.is_method )
				return String.Format("{0}({1})", this.param, this.parent.key );
			return this.key;
		}

		public Filter eq(string val) {
			return new Filter(this, val, "");
		}

		public Filter ne(string val) {
			return new Filter(this, val, "!");
		}

		public Filter gt(string val) {
			return new Filter(this, val, ">");
		}

		public Filter lt(string val) {
			return new Filter(this, val, "<");
		}

		public Filter ge(string val) {
			return new Filter(this, val, ">=");
		}

		public Filter le(string val) {
			return new Filter(this, val, "<=");
		}

		public Filter contains(string val) {
			return new Filter(this, val, "?*");
		}

	}
}
