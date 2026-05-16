using Nijo.Util.CodeGenerating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Core.AggregateMemberTypes {
    /// <summary>
    /// シーケンス型のメンバー。ルート集約にのみ定義可能。
    /// </summary>
    internal class SequenceMember : SchalarMemberType {

        internal const char SEQ_SEPARATOR = ',';

        protected override string ComponentName => "Input.Num";

        public override string GetCSharpTypeName() => "int";

        public override string GetHelpText() {
            return $$"""
                シーケンスメンバー。
                新規作成画面の表示時や、一括編集画面の行追加時など、
                SQL発行のタイミングで発番されます。
                型詳細で "シーケンス物理名" + "{{SEQ_SEPARATOR}}" + "桁数" のように詳細を指定してください（桁数は省略可能）。
                """;
        }

        public override string GetTypeScriptTypeName() => "number | null";

        public override string GetUiDisplayName() => "シーケンス";

        public override string UiConstraintType => "NumberMemberConstraint";

        private protected override IEnumerable<string> RenderAttributes(AggregateMember.ValueMember vm, FormUIRenderingContext ctx) {
            yield return $"className=\"w-28\"";
        }
        public override string DataTableColumnDefHelperName => "sequenceMember";

        public override WijmoGridColumnSetting GetWijmoGridColumnSetting() => new WijmoGridColumnSetting {
            DataType = "Number",
            Format = "d",
        };

        public override IEnumerable<string> RenderUiConstraintValue(AggregateMember.ValueMember vm) {
            if (vm.Options.TotalDigits != null) {
                yield return $"totalDigit: {vm.Options.TotalDigits}";
            }
        }
    }
}
