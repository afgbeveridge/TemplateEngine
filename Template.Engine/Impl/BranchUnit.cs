
namespace Infra.BTA.Templates {

	public class BranchUnit : ExecutionUnit {

		public override void Accept(IExecutableObject obj) {
			var alt = obj as AlternativeUnit;
			// Assert that obj == null || !InAlternate
			if (alt != null) {
				InAlternateState = true;
				Alternate = alt;
			}
			else {
				if (InAlternateState)
					Alternate.Accept(obj);
				else
					base.Accept(obj);
			}
		}

		public override void Execute(ExecutionContext ctx) {
			bool result = new ExpressionEngine().EvaluateConditional(ctx, Content);
			if (result)
				base.Execute(ctx);
			else if (Alternate != null)
				Alternate.Execute(ctx);

		}

		public override string Keyword {
			get {
				return "if";
			}
		}

		private bool InAlternateState { get; set; }

		private ExecutionUnit Alternate { get; set; }

	}
}
