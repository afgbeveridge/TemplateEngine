using System.Collections.Generic;
using System.IO;

namespace Infra.BTA.Templates {
	
	public class ParseContext {

		public ParseContext(TextReader reader, string pattern = null) : this(reader.ReadToEnd(), pattern) {
		}

		public ParseContext(string content, string pattern = null) {
			Content = content;
			Pattern = pattern ?? StandardRegexPattern;
			Units = new Stack<IExecutionUnit>();
		}

		public string Pattern { get; private set; }

		public string Content { get; private set; }

		public bool PerfectMatching { get; set; }

		public IExecutionUnit TOS {
			get {
				return Units.Peek();
			}
		}

		public void Push(IExecutionUnit unit) {
			Units.Push(unit);
		}

		public void Pop() {
			Units.Pop();
		}

		public int StackedUnits {
			get {
				return Units.Count;
			}
		}

		public const string StandardRegexPattern = @"\[(?<key>[^\]]+)\]";

        public static string GenerateEscapedPattern(char escapeCharacter, bool atEndAsWell = true) {
            var escape = $"(?=[^{escapeCharacter}]{{1}})"; // Match but do not capture as this would affect text substitution later on
            return escape + StandardRegexPattern + (atEndAsWell ? escape : string.Empty);
        }

		private Stack<IExecutionUnit> Units { get; set; }

	}
}
