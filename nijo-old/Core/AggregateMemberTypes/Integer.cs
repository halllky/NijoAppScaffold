using Nijo.Util.CodeGenerating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Core.AggregateMemberTypes {
    internal class Integer : SchalarMemberType {
        public override string GetUiDisplayName() => "整数";
        public override string GetHelpText() => $"整数。";

        public override string GetCSharpTypeName() => "int";
        public override string GetTypeScriptTypeName() => "string | null";

        public override string UiConstraintType => "NumberMemberConstraint";

        protected override string ComponentName => "Input.Num";
        private protected override IEnumerable<string> RenderAttributes(AggregateMember.ValueMember vm, FormUIRenderingContext ctx) {
            yield return $"className=\"w-28\"";
        }

        public override WijmoGridColumnSetting GetWijmoGridColumnSetting() => new WijmoGridColumnSetting {
            DataType = "String",
            Format = "d",
        };

        public override string DataTableColumnDefHelperName => "integer";

        public override IEnumerable<string> RenderUiConstraintValue(AggregateMember.ValueMember vm) {
            if (vm.Options.TotalDigits != null) {
                yield return $"totalDigit: {vm.Options.TotalDigits}";
            }
        }
    }
}
