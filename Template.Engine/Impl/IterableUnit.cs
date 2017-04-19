using System;
using System.Collections;
using System.Linq;

namespace Infra.BTA.Templates {
	
	public class IterableUnit : ExecutionUnit {

		public override string Keyword {
			get {
				return "foreach";
			}
		}

		public override void Execute(ExecutionContext ctx) {
			string iterableName = GetComponentOfContent();
			IEnumerable iter = ctx.AmbientState.GetEnumerable(iterableName);
			if (iter != null) {
				var iterator = iter.GetEnumerator();
				while (iterator.MoveNext()) {
					ctx.AmbientState.PushContext(iterator.Current);
					Children.ForEach(c => c.Execute(ctx));
					ctx.AmbientState.PopContext();
				}
			}
		}
	}
}
