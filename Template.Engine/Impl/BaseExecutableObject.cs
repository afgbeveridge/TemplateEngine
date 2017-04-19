using System;
using System.Collections.Generic;
using System.Linq;

namespace Infra.BTA.Templates {
	
	public abstract class BaseExecutableObject : IExecutableObject {

		protected BaseExecutableObject(string content = null) {
			Content = content;
		}

        public virtual bool Understands(string encoding, IEnumerable<string> split) {
            return split.FirstOrDefault() == Keyword;
        }

        public abstract void Execute(ExecutionContext ctx);

		public string Content { get; set; }

		public virtual bool IsUnitScoped {
			get { return false; }
		}

		public virtual bool EndsScope {
			get {
				return false;
			}
		}

		public virtual bool KeywordBased {
			get {
				return false;
			}
		}

		public virtual string Keyword {
			get {
				return null;
			}
		}

        protected string GetComponentOfContent(int idx = 1) {
            return Content.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ElementAtOrDefault(idx);
        }
	}
}
