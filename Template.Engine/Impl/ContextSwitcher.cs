
namespace Infra.BTA.Templates {

    public class ContextSwitcher : BaseExecutableObject {

        public override void Execute(ExecutionContext ctx) {
            ctx.AmbientState.SwitchContext(GetComponentOfContent());
        }

        public override bool KeywordBased {
            get {
                return true;
            }
        }

        public override string Keyword {
            get {
                return "context";
            }
        }
    }
}
