using System.Collections.Generic;

namespace Infra.BTA.Templates {
	
	public interface IExecutableObject {
		bool Understands(string encoding, IEnumerable<string> split);
		void Execute(ExecutionContext ctx);
		string Content { get; set; }
		bool IsUnitScoped { get; }
		bool EndsScope { get; }
		bool KeywordBased { get; }
		string Keyword { get; }
	}

	public interface IExecutionUnit {
		void Accept(IExecutableObject obj);
	}
}
