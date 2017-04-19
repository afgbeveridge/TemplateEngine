using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Reflection;

namespace Infra.BTA.Templates {
	
	public class EvaluationContext {

		public EvaluationContext(object root, string name = null) : this(new ContextObject(root), name) {
		}

		public EvaluationContext(IContextObject ctxObject, string name = null) {
			ContextObjects = new List<IContextObject> { ctxObject };
            AddNamedContext(ctxObject, name);
		}

        public EvaluationContext AddNamedContext(object obj, string name = null) {
            return AddNamedContext(new ContextObject(obj), name);
        }

        public EvaluationContext AddNamedContext(IContextObject obj, string name = null) {
            NamedContextObjects[name ?? obj.Identifier] = obj;
            return this;
        }

        internal void SwitchContext(string name) {
            ContextObjects[0] = NamedContextObjects[name];
        }

        internal void PushContext(object obj) {
			ContextObjects.Insert(0, CreateContext(obj));
		}

		internal void PopContext() {
			ContextObjects.RemoveAt(0);
		}

		public IContextObject TOS {
			get {
				return ContextObjects.First();
			}
		}

		public string GetValue(string property, string defaultValue = "") {
			var obj = FindWith(property);
			return obj == null ? String.Empty : (obj.GetValue(property) ?? defaultValue);
		}

		public IEnumerable GetEnumerable(string property) {
			var obj = FindWith(property);
			return obj == null ? null : obj.GetEnumerable(property);
		}

        public static EvaluationContext From(params object[] objects) {
            var ctx = new EvaluationContext(objects.First());
            objects.Skip(1).ToList().ForEach(obj => ctx.AddNamedContext(obj));
            return ctx;
        }

        private IContextObject FindWith(string property) {
			return ContextObjects.FirstOrDefault(co => co.CanNavigate(property));
		}

		private IContextObject CreateContext(object obj) {
			Type ctxType = ContextObjects.Last().GetType();
			var ctor = ctxType.GetTypeInfo().GetConstructor(new [] { typeof(object) });
			return ctor.Invoke(new [] { obj }) as IContextObject;
		}

		private List<IContextObject> ContextObjects { get; set; }

        private Dictionary<string, IContextObject> NamedContextObjects { get; set; } = new Dictionary<string, IContextObject>();
	}
}
