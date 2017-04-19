using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Infra.BTA.Templates {
	
	public class ParseResult {

		public ParseResult() {
			Errors = Enumerable.Empty<TemplateError>().ToList();
		}

		public IExecutableObject Executable { get; set; }

        public (ExecutionContext context, long elapsed) Execute(ExecutionContext ctx) {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            Executable.Execute(ctx);
            watch.Stop();
            return (context: ctx, elapsed: watch.ElapsedMilliseconds);
        }

        public bool Success { 
			get {
				return !Errors.Any();
			} 
		}

		public IEnumerable<TemplateError> RecordedErrors { get { return Errors; } }

		public void AddError(TemplateError err) {
			Errors.Add(err);
		}

		public void AddError(string err) {
			Errors.Add(new TemplateError(err));
		}

		private List<TemplateError> Errors { get; set; }

	}

}
