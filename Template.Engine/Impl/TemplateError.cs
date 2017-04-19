using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Infra.BTA.Templates {
	
	public class TemplateError {

		public TemplateError(string text, int code = -1) {
			Code = code;
			Message = text;
		}

		public int Code { get; private set; }

		public string Message { get; private set; }

		public int SourcePosition { get; set; }

	}
}
