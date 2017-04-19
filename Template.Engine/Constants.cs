using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Infra.BTA {
	
	public static class Constants {

		public static class Engine { 
			
			public static Regex ConditionalTest =
				new Regex("\\w+(\\.\\w+)*|[+-]?(?:\\d+\\.?\\d*|\\d*\\.?\\d+)[\\r\\n]*|<|>|==|!=|<=|>=|\"[^\"]*\"|true|false|null");
			public static Tuple<string, string> Identifier = Tuple.Create("^\\w+(\\.\\w+)*$", "Identifier");
			// Operators
			public static Tuple<string, string> GreaterThan = Tuple.Create(">", "Operator");
			public static Tuple<string, string> GTE = Tuple.Create(">=", "Operator");
			public static Tuple<string, string> LessThan = Tuple.Create("<", "Operator");
			public static Tuple<string, string> LTE = Tuple.Create("<=", "Operator");
			public static Tuple<string, string> EqualsOperator = Tuple.Create("==", "Operator");
			public static Tuple<string, string> NotEquals = Tuple.Create("!=", "Operator");
			// Numerics
			public static Tuple<string, string> Number = Tuple.Create("[+-]?(?:\\d+\\.?\\d*|\\d*\\.?\\d+)[\\r\\n]*", "Number");
			// string
			public static Tuple<string, string> StringType = Tuple.Create("\"[^\"]*\"", "String");
			// Misc
			public static Tuple<string, string> BooleanType = Tuple.Create("true|false", "Boolean");
			public static Tuple<string, string> NullType = Tuple.Create("true|false", "Null");
			// Reference
			public static List<Tuple<string, string>> AllTokens = new List<Tuple<string, string>> { 
				GreaterThan, GTE, LessThan, LTE, EqualsOperator, NotEquals, StringType, Number, BooleanType, NullType, Identifier
			};
		
		}

	}
}
