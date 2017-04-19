using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Infra.BTA.Templates {

    public class ExpressionEngine {

        public ExpressionEngine() {
            ExecutionState = new Queue<ExpressionObject>();
        }

        public bool EvaluateConditional(ExecutionContext ctx, string src, int skipFirst = 1) {
            MatchCollection c = Constants.Engine.ConditionalTest.Matches(src);
            // Constants.Engine.AllTokens.FirstOrDefault(t => Regex.IsMatch(token, t.Item1));
            // Get Item2 from this - can form a type name, and build, and set content
            // Enqueue these objects
            // while (queue size > 1)
            // deq, add to eval object
            // if eval object can execute, make it do so, enq its return value
            // and so on
            foreach (Match m in c) {
                if (skipFirst-- <= 0) {
                    var token = m.Value;
                    var tok = Constants.Engine.AllTokens.FirstOrDefault(t => Regex.IsMatch(token, t.Item1));
                    ExecutionState.Enqueue(CreateExpressionObject(tok.Item2, m.Value));
                }
            }
            return Execute(ctx).As<bool>(ctx);
        }

        private ExpressionObject CreateExpressionObject(string tag, string value) {
            var name = string.Concat(tag, "ExpressionObject");
            var type = typeof(ExpressionObject).GetTypeInfo().Assembly.GetTypes().FirstOrDefault(t => t.Name == name);
            var ctor = type.GetConstructor(Type.EmptyTypes);
            var result = ctor.Invoke(null) as ExpressionObject;
            result.Source = value;
            return result;
        }

        private Queue<ExpressionObject> ExecutionState { get; set; }

        private ExpressionObject Execute(ExecutionContext ctx) {
            var binop = new BinaryExpressionEvaluator();
            while (ExecutionState.Count > 0) {
                binop.Accept(ExecutionState.Dequeue());
                if (binop.CanEvaluate) {
                    ExecutionState.Enqueue(binop.Evaluate(ctx));
                    binop.Reset();
                }
            }
            return binop.Lhs;
        }

    }

    internal class BinaryExpressionEvaluator {

        internal void Reset() {
            Lhs = Operator = Rhs = null;
        }

        internal ExpressionObject Evaluate(ExecutionContext ctx) {
            return ((OperatorExpressionObject)Operator).Evaluate(ctx, Lhs, Rhs);
        }

        internal bool CanEvaluate {
            get {
                return Lhs != null && Operator != null && Rhs != null;
            }
        }

        internal void Accept(ExpressionObject obj) {
            if (Lhs == null)
                Lhs = obj;
            else if (Operator == null)
                Operator = obj;
            else
                Rhs = obj;
        }

        internal ExpressionObject Lhs { get; set; }

        private ExpressionObject Operator { get; set; }

        private ExpressionObject Rhs { get; set; }

    }

    internal abstract class ExpressionObject {

        protected ExpressionObject() { }

        internal virtual string Source { get; set; }

        internal virtual object Value(ExecutionContext ctx) {
            return Source;
        }

        internal virtual bool IsNull(ExecutionContext ctx) {
            return Value(ctx) == null;
        }

        internal virtual bool IsBoolean {
            get {
                return false;
            }
        }

        internal virtual bool IsString {
            get {
                return false;
            }
        }

        internal virtual bool IsNumber {
            get {
                return false;
            }
        }

        internal T As<T>(ExecutionContext ctx) {
            return (T)Convert.ChangeType(Value(ctx), typeof(T));
        }
    }

    internal class NumberExpressionObject : ExpressionObject {

        internal override bool IsNumber {
            get {
                return true;
            }
        }

        internal static bool CouldParse(object obj) {
            decimal result;
            return decimal.TryParse(obj == null ? String.Empty : obj.ToString(), out result);
        }

    }

    internal class BooleanExpressionObject : ExpressionObject {

        internal override bool IsBoolean {
            get {
                return true;
            }
        }

        internal static bool CouldParse(object obj) {
            bool result;
            return bool.TryParse(obj == null ? String.Empty : obj.ToString(), out result);
        }

    }

    internal class IdentifierExpressionObject : ExpressionObject {

        internal override bool IsBoolean {
            get {
                return true;
            }
        }

        internal override object Value(ExecutionContext ctx) {
            return ctx.AmbientState.GetValue(Source, null);
        }

    }

    internal class NullExpressionObject : ExpressionObject {

        internal override bool IsNull(ExecutionContext ctx) {
            return true;
        }

    }

    internal class StringExpressionObject : ExpressionObject {

        internal override string Source {
            get {
                return base.Source;
            }
            set {
                // Trim start and end quotes
                base.Source = value == null ? null : value.Substring(1, value.Length - 2);
            }
        }

        internal override bool IsString {
            get {
                return true;
            }
        }
    }

    internal class OperatorExpressionObject : ExpressionObject {

        private static Func<object, decimal> AsDecimal = obj => (decimal)Convert.ChangeType(obj, typeof(decimal));
        private static Func<object, Type, object> ChangeType = (obj, t) => Convert.ChangeType(obj, t);
        private static Func<bool, ExpressionObject> BooleanResult = b => new BooleanExpressionObject { Source = b.ToString() };

        private static Dictionary<string, Func<ExecutionContext, ExpressionObject, ExpressionObject, ExpressionObject>> Handlers = new Dictionary<string, Func<ExecutionContext, ExpressionObject, ExpressionObject, ExpressionObject>> {
            { ">", (ctx, lhs, rhs) => BooleanResult(AsDecimal(lhs.Value(ctx)) > AsDecimal(rhs.Value(ctx))) },
            { ">=", (ctx, lhs, rhs) => BooleanResult(AsDecimal(lhs.Value(ctx)) >= AsDecimal(rhs.Value(ctx))) },
            { "<", (ctx, lhs, rhs) => BooleanResult(AsDecimal(lhs.Value(ctx)) < AsDecimal(rhs.Value(ctx))) },
            { "<=", (ctx, lhs, rhs) => BooleanResult(AsDecimal(lhs.Value(ctx)) <= AsDecimal(rhs.Value(ctx))) },
            { "==", (ctx, lhs, rhs) => {
                var ops = ProcessOperands(ctx, lhs, rhs);
                return BooleanResult((ops.left == null && ops.right == null) ||
                                    (ops.left != null ? ops.left.Equals(ops.right) : ops.right.Equals(ops.left)));
                }
            },
            { "!=", (ctx, lhs, rhs) => BooleanResult(!Handlers["=="](ctx, lhs, rhs).As<bool>(ctx)) }
        };

        internal ExpressionObject Evaluate(ExecutionContext ctx, ExpressionObject lhs, ExpressionObject rhs) {
            return Handlers[Source](ctx, lhs, rhs);
        }

        private static (object left, object right) ProcessOperands(ExecutionContext ctx, ExpressionObject lhs, ExpressionObject rhs) {
            object lhsVal = lhs.Value(ctx);
            Type target = lhs.IsNull(ctx) ? typeof(string) :
                (BooleanExpressionObject.CouldParse(lhsVal) ?
                    typeof(bool) :
                    (NumberExpressionObject.CouldParse(lhsVal) ? typeof(decimal) : typeof(string)));
            return (left: ChangeType(lhsVal, target), right: ChangeType(rhs.Value(ctx), target));
        }

    }
}
