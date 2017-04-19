using System.Collections.Generic;

namespace Infra.BTA.Templates {
	
	public abstract class ExecutionUnit : BaseExecutableObject, IExecutionUnit {

		public ExecutionUnit() {
			Children = new List<IExecutableObject>();
		}

		public override void Execute(ExecutionContext ctx) {
			Children.ForEach(c => c.Execute(ctx));
		}

		public override bool IsUnitScoped {
			get { return true; }
		}

		public virtual void Accept(IExecutableObject obj) {
			Children.Add(obj);
		}

		public override bool KeywordBased {
			get {
				return true;
			}
		}

		protected List<IExecutableObject> Children { get; set; }

	}
}
