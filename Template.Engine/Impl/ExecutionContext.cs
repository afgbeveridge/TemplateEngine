using System;
using System.IO;

namespace Infra.BTA.Templates {
	
	public class ExecutionContext {

		public ExecutionContext(TextWriter writer, EvaluationContext ctx) {
			Sink = writer;
			AmbientState = ctx;
		}

		public TextWriter Sink { get; private set; }

		public EvaluationContext AmbientState { get; private set; }

		public override string ToString() {
			return Sink == null ? String.Empty : Sink.ToString();
		}

		public static ExecutionContext Build(object root) {
			return Build(new EvaluationContext(root));
		}

        public static ExecutionContext Build(EvaluationContext ctx) {
            return new ExecutionContext(new StringWriter(), ctx);
        }

    }

}
