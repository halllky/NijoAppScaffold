using Nijo.Parts.WebServer;
using Nijo.Parts.BothOfClientAndServer;
using Nijo.Util.CodeGenerating;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Core.AggregateMemberTypes {
    internal class YearMonth : SchalarMemberType {
        public override string GetUiDisplayName() => "年月";
        public override string GetHelpText() => $"年月。";

        public override void GenerateCode(CodeRenderingContext context) {
            // クラス定義
            context.CoreLibrary.UtilDir(dir => {
                dir.Generate(RuntimeYearMonthClass.RenderDeclaring());
            });

            // JavaScriptとC#の間の変換
            var util = context.UseSummarizedFile<UtilityClass>();
            util.AddJsonConverter(RuntimeYearMonthClass.GetCustomJsonConverter(context));

            // C#とDBの間の変換
            context.UseSummarizedFile<Parts.WebServer.DbContextClass>()
                .AddOnModelCreatingPropConverter(RuntimeYearMonthClass.CLASS_NAME, "GetYearMonthEFCoreValueConverter");
            context.UseSummarizedFile<Parts.Configure>().AddMethod($$"""
                /// <summary>
                /// <see cref="{{RuntimeYearMonthClass.CLASS_NAME}}"/> クラスのプロパティがDBとC#の間で変換されるときの処理を定義するクラスを返します。
                /// </summary>
                public virtual Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter GetYearMonthEFCoreValueConverter() {
                    return new {{RuntimeYearMonthClass.EFCoreConverterClassFullName}}();
                }
                """);
        }

        public override string GetCSharpTypeName() => RuntimeYearMonthClass.CLASS_NAME;
        public override string GetTypeScriptTypeName() => YearMonthDay.CurrentCodeRenderingContext?.Config.UseWijmo == true ? "Date | null" : "number | null";

        protected override string ComponentName => "Input.YearMonth";
        private protected override IEnumerable<string> RenderAttributes(AggregateMember.ValueMember vm, FormUIRenderingContext ctx) {
            if (YearMonthDay.CurrentCodeRenderingContext?.Config.UseWijmo != true) {
                yield return $"className=\"w-20\"";
            }
        }

        public override WijmoGridColumnSetting GetWijmoGridColumnSetting() => new WijmoGridColumnSetting {
            DataType = "string",
            Format = "null",
        };

        private protected override string RenderSearchConditionVFormBody(AggregateMember.ValueMember vm, FormUIRenderingContext ctx) {
            if (ctx.CodeRenderingContext.Config.UseWijmo) {
                var fullpath = ctx.GetReactHookFormFieldPath(vm.Declared).Join(".");

                //Wijmo
                return $$"""
                <div className="flex flex-wrap items-center">
                  <{{ComponentName}} {...{{ctx.Register}}(`{{fullpath}}.{{FromTo.FROM_TS}}`)} {{RenderAttributes(vm, ctx).Select(x => $"{x} ").Join("")}}/>
                  <span className="select-none">～</span>
                  <{{ComponentName}} {...{{ctx.Register}}(`{{fullpath}}.{{FromTo.TO_TS}}`)} {{RenderAttributes(vm, ctx).Select(x => $"{x} ").Join("")}}/>
                </div>
                """;

            } else {
                // Wijmoを使わない場合はnijo標準の「（開始日）～（終了日）」のUIを使う
                return base.RenderSearchConditionVFormBody(vm, ctx);
            }
        }

        public override string DataTableColumnDefHelperName => "yearMonth";
    }
}
