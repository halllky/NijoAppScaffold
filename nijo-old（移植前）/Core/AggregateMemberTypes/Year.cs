using Nijo.Util.CodeGenerating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Core.AggregateMemberTypes {
    internal class Year : SchalarMemberType {
        public override string GetUiDisplayName() => "年";
        public override string GetHelpText() => $"年。西暦で登録されます。";

        public override string GetCSharpTypeName() => "int";
        public override string GetTypeScriptTypeName() => "number | null";

        protected override string ComponentName => "Input.Num";
        private protected override IEnumerable<string> RenderAttributes(AggregateMember.ValueMember vm, FormUIRenderingContext ctx) {
            yield return $"className=\"w-16\"";
            yield return $"placeholder=\"0000\"";
        }

        public override WijmoGridColumnSetting GetWijmoGridColumnSetting() => new WijmoGridColumnSetting {
            DataType = "Number",
            Format = "d",
        };

        public override string DataTableColumnDefHelperName => "year";
    }
}
