using System.Collections.Generic;
using System.Collections;

namespace Infra.BTA.Templates {
	
	public class DictionaryContextObject : IContextObject {

		public DictionaryContextObject() {
			Values = new Dictionary<string, object>();
		}

		public DictionaryContextObject(object obj) {
			var dco = obj as DictionaryContextObject;
			if (dco != null)
				Values = dco.Values;
			else { 
				Values = new Dictionary<string, object>();
				Values[PropertyReference.ImplicitReference] = obj;
			}
		}

		public bool CanNavigate(string property) {
			return Values.ContainsKey(property);
		}

		public IEnumerable GetEnumerable(string property) {
			return CanNavigate(property) ? (IEnumerable)Values[property] : null;
		}

		public object GetObject(string property) {
			return CanNavigate(property) ? Values[property] : null;
		}

		public string GetValue(string property) {
			return CanNavigate(property) ? Values[property].ToString() : null;
		}

		public DictionaryContextObject Add(string key, object val) {
			Values[key] = val;
			return this;
		}

        public string Identifier => Values.GetType().Name;

        private Dictionary<string, object> Values { get; set; }
	}
}
