
namespace Infra.BTA.Templates {

	public class AlternativeUnit : ExecutionUnit {

		public override bool IsUnitScoped {
			get { return false; }
		}

		public override string Keyword {
			get {
				return "else";
			}
		}

	}
}
