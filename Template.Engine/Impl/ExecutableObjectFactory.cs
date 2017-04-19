using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Infra.BTA.Templates {
	
	public static class ExecutableObjectFactory {

		private static List<IExecutableObject> RegisteredObjects = new List<IExecutableObject>();

		private static List<string> KnownKeywords = new List<string>();

        private static List<string> AssembliesProbed { get; set; } = new List<string>();

		public static IExecutableObject Realize(string encoding, IEnumerable<string> split) {
			IExecutableObject match = RegisteredObjects.FirstOrDefault(obj => obj.Understands(encoding, split));
			return match == null ? null : Activator.CreateInstance(match.GetType()) as IExecutableObject;
		}

        public static void RegisterFromSelf() {
            RegisterFrom(typeof(ExecutableObjectFactory).GetTypeInfo().Assembly);
        }

        public static void RegisterFrom(Assembly ass) {
            if (!AssembliesProbed.Any(s => s == ass.FullName)) {
                RegisteredObjects.AddRange(ass
                    .GetTypes()
                    .Where(t => !t.GetTypeInfo().IsAbstract && t.GetInterfaces().Contains(typeof(IExecutableObject)))
                    .Select(t => Activator.CreateInstance(t) as IExecutableObject));
                RegisteredObjects = RegisteredObjects.OrderByDescending(e => e.KeywordBased).ToList();
                KnownKeywords = RegisteredObjects.Where(eo => eo.KeywordBased && eo.Keyword != null).Select(eo => eo.Keyword).ToList();
                AssembliesProbed.Add(ass.FullName);
            }
		}

        public static IEnumerable<IExecutableObject> KnownObjects => RegisteredObjects;

	}
}
