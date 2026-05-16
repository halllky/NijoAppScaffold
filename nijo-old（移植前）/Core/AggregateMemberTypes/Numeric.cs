using Nijo.Util.CodeGenerating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Core.AggregateMemberTypes {
    internal class Numeric : SchalarMemberType {
        public override string GetUiDisplayName() => "実数";
        public override string GetHelpText() => $"実数。";

        public override string GetCSharpTypeName() => "decimal";
        public override string GetTypeScriptTypeName() => "string | null";

        public override string UiConstraintType => "NumberMemberConstraint";

        protected override string ComponentName => "Input.Num";

        private protected override IEnumerable<string> RenderAttributes(AggregateMember.ValueMember vm, FormUIRenderingContext ctx) {
            yield return $"className=\"w-28\"";
            yield return $"totaldigits={{{vm.Options.TotalDigits}}}"; // 実数値の全体桁数
            yield return $"fractionaldigits={{{vm.Options.FractionalDigits}}}"; // 実数値の少数部桁数
        }

        public override string DataTableColumnDefHelperName => "numeric";
        public override WijmoGridColumnSetting GetWijmoGridColumnSetting() => new WijmoGridColumnSetting {
            DataType = "String",
            Format = "f",
        };

        public override IEnumerable<string> RenderUiConstraintValue(AggregateMember.ValueMember vm) {
            if (vm.Options.TotalDigits != null) {
                yield return $"totalDigit: {vm.Options.TotalDigits}";
            }
            if (vm.Options.FractionalDigits != null) {
                yield return $"decimalPlace: {vm.Options.FractionalDigits}";
            }
            if (vm.Options.NotNegative) {
                yield return $"notNegative: true";
            }
        }
    }
}
