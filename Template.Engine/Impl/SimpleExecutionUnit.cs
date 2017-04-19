using System.Collections.Generic;

namespace Infra.BTA.Templates {
	
	public class SimpleExecutionUnit : ExecutionUnit {

		public override bool Understands(string encoding, IEnumerable<string> split) {
			return false;
		}
	}
}
