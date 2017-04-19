using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Infra.BTA.Templates {
	
	public class Parser {

		private ParseContext ParsingContext { get; set; }

		private ParseResult Result { get; set; }

        public static ParseResult Parse(string content, string pattern = null) {
            return new Parser().Parse(new ParseContext(content, pattern));
        }

        public ParseResult Parse(ParseContext ctx) {
			try {
				ParsingContext = ctx;
				Result = new ParseResult();
				ctx.Push(new SimpleExecutionUnit());
				IEnumerable<Match> matches = Regex.Matches(ctx.Content, ctx.Pattern).Cast<Match>();
				matches.ToList().ForEach(Process);
				CheckLast(matches.Last());
				Result.Executable = ctx.TOS as IExecutableObject;
			}
			catch (Exception ex) {
				Result.AddError(string.Concat("An unexpected error occurred during parsing: ", ex.ToString()));
			}
			return Result;
		}

		private void Process(Match m) {
			int matchIndex = m.Index;
			if (matchIndex - SourceIndex > 0)
				ParsingContext.TOS.Accept(new LiteralExecutableObject(ParsingContext.Content.Substring(SourceIndex, matchIndex - SourceIndex)));
			string cur = m.Groups["key"].Value;
			var exeObject = ExecutableObjectFactory.Realize(cur, cur.Split(' '));
			if (exeObject != null)
				Absorb(m, cur, exeObject);
			else
				AnalyzeMismatch(m, cur, exeObject);
		}

		private void Absorb(Match m, string cur, IExecutableObject exeObject) {
			ParsingContext.TOS.Accept(exeObject);
			exeObject.Content = cur;
			SourceIndex = m.Index + m.Value.Length;
			if (exeObject.IsUnitScoped)
				ParsingContext.Push(exeObject as IExecutionUnit);
			if (exeObject.EndsScope)
				ParsingContext.Pop();
		}

		private void AnalyzeMismatch(Match m, string cur, IExecutableObject exeObject) {
			if (ParsingContext.PerfectMatching)
				Result.AddError(new TemplateError(string.Concat("Unrecognised token: \"", cur, "\"")) { SourcePosition = m.Index }); 
			else
				ParsingContext.TOS.Accept(new LiteralExecutableObject(m.Value));
		}

		private void CheckLast(Match m) {
			var idx = m.Index + m.Value.Length;
			if (idx < ParsingContext.Content.Length - 1)
				ParsingContext.TOS.Accept(new LiteralExecutableObject(ParsingContext.Content.Substring(idx)));
			if (ParsingContext.StackedUnits != 1)
				Result.AddError("One or more statements are unterminated");
		}

		private int SourceIndex { get; set; }

	}
}
