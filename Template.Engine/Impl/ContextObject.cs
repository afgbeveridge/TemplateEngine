using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Infra.BTA.Templates {

    public class ContextObject : IContextObject {

        public ContextObject(object root) {
            Root = root;
        }

        public string GetValue(string property) {
            var root = property == PropertyReference.ImplicitReference ? Root : GetObject(property);
            return root?.ToString();
        }

        public IEnumerable GetEnumerable(string property) {
            return (IEnumerable)GetObject(property);
        }

        public object GetObject(string property) {
            return Traverse(Root, Deconstruct(property), (inf, obj) => inf?.GetValue(obj, null), o => o.GetType());
        }

        public string Identifier => Root.GetType().Name;

        private TObject Traverse<TObject>(TObject cur, IEnumerable<string> propertyList, Func<PropertyInfo, TObject, TObject> f, Func<TObject, Type> g, int idx = 0) where TObject : class {
            return cur == default(TObject) || idx >= propertyList.Count() ?
                    cur :
                    Traverse(f(g(cur).GetProperty(propertyList.ElementAt(idx)), cur), propertyList, f, g, idx + 1);
        }

        public bool CanNavigate(string property) {
            return property == PropertyReference.ImplicitReference || Traverse(Root.GetType(), Deconstruct(property), (inf, obj) => inf?.PropertyType, o => o) != null;
        }

        private string[] Deconstruct(string property) {
            return property.Split('.');
        }

        private object Root { get; set; }

    }
}
