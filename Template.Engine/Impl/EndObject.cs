
namespace Infra.BTA.Templates {
	
	public class EndObject : BaseExecutableObject {

		public override void Execute(ExecutionContext ctx) { }

		public override bool EndsScope {
			get {
				return true;
			}
		}

		public override bool KeywordBased {
			get {
				return true;
			}
		}

		public override string Keyword {
			get {
				return "end";
			}
		}
	}
}
