using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Infra.BTA.Templates {

	public class PropertyReference : BaseExecutableObject {

		public const string ImplicitReference = "_";

		public override bool Understands(string encoding, IEnumerable<string> split) {
			string target = split.FirstOrDefault() ?? string.Empty;
			return target == ImplicitReference || Regex.IsMatch(encoding, Constants.Engine.Identifier.Item1);
		}

		public override void Execute(ExecutionContext ctx) {
			ctx.Sink.Write(ctx.AmbientState.GetValue(Content));
		}
	}
}
