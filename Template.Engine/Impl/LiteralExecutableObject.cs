using System.Collections.Generic;

namespace Infra.BTA.Templates {
	
	public class LiteralExecutableObject : BaseExecutableObject {

		public LiteralExecutableObject() : this(null) {
		}

		public LiteralExecutableObject(string content) : base(content) {
		}
	
		public override void Execute(ExecutionContext ctx) {
			ctx.Sink.Write(Content);
		}

		public override bool Understands(string encoding, IEnumerable<string> split) {
			return false;
		}

	}
}
