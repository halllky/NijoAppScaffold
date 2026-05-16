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
    internal class YearMonthDay : SchalarMemberType {
        public override string GetUiDisplayName() => "日付";
        public override string GetHelpText() => $"年月日。";

        public override void GenerateCode(CodeRenderingContext context) {
            // クラス定義
            context.CoreLibrary.UtilDir(dir => {
                dir.Generate(RuntimeDateClass.RenderDeclaring());
            });

            // JavaScriptとC#の間の変換
            var util = context.UseSummarizedFile<UtilityClass>();
            util.AddJsonConverter(RuntimeDateClass.GetCustomJsonConverter());

            // C#とDBの間の変換
            context.UseSummarizedFile<Parts.WebServer.DbContextClass>()
                .AddOnModelCreatingPropConverter(RuntimeDateClass.CLASS_NAME, "GetYearMonthDayEFCoreValueConverter");
            context.UseSummarizedFile<Parts.Configure>().AddMethod($$"""
                /// <summary>
                /// <see cref="{{RuntimeDateClass.CLASS_NAME}}"/> クラスのプロパティがDBとC#の間で変換されるときの処理を定義するクラスを返します。
                /// </summary>
                public virtual Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter GetYearMonthDayEFCoreValueConverter() {
                    return new {{RuntimeDateClass.EFCoreConverterClassFullName}}();
                }
                """);
        }

        #region Wijmo用対応
        /// <summary>
        /// nijo標準では日付はstirngだが、WijmoではDate。
        /// 本当は <see cref="GetTypeScriptTypeName"/> の中で <see cref="Config.UseWijmo"/> の値を見てstringにするかDateにするかを分けたいが、
        /// <see cref="GetTypeScriptTypeName"/> メソッドに <see cref="CodeRenderingContext"/> を渡すのが大変なので、やむなしでシングルトンで持たせている。 
        /// </summary>
        public static CodeRenderingContext? CurrentCodeRenderingContext { get; set; }
        #endregion Wijmo用対応

        public override string GetCSharpTypeName() => RuntimeDateClass.CLASS_NAME;
        public override string GetTypeScriptTypeName() => CurrentCodeRenderingContext?.Config.UseWijmo == true ? "Date | null" : "string";

        protected override string ComponentName => CurrentCodeRenderingContext?.Config.UseWijmo == true ? "Input.DateInput" : "Input.Date";
        private protected override IEnumerable<string> RenderAttributes(AggregateMember.ValueMember vm, FormUIRenderingContext ctx) {

            if (CurrentCodeRenderingContext?.Config.UseWijmo != true) {
                yield return $"className=\"w-96\"";
            }

        }

        public override WijmoGridColumnSetting GetWijmoGridColumnSetting() => new WijmoGridColumnSetting {
            DataType = "String",
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

        public override string DataTableColumnDefHelperName => "date";
    }
}
