using Nijo.Models.ReadModel2Features;
using Nijo.Models.RefTo;
using Nijo.Models.WriteModel2Features;
using Nijo.Util.CodeGenerating;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Core.AggregateMemberTypes {
    /// <summary>
    /// 添付ファイル型。
    /// 実行時の型はアプリケーションテンプレート側のプロジェクトで定義しています。
    /// </summary>
    internal class FileAttachment : IAggregateMemberType {

        // 添付ファイルのメタデータはDBにJSONで保存されるので、そのJSONのプロパティ名
        private const string C_METADATA = "metadata";
        private const string C_WILL_DETACH = "willDetach";
        private const string C_DISPLAY_FILE_NAME = "displayFileName";
        private const string C_HREF = "href";
        private const string C_DOWNLOAD = "download";
        private const string C_OTHER_PROPS = "othreProps";

        public const string C_FILE_ATTACHMENT_ID = "fileAttachmentId";

        public string GetCSharpTypeName() => "List<FileAttachmentMetadata>";
        public string GetTypeScriptTypeName() => "Util.FileAttachmentMetadata";

        public string UiConstraintType => "MemberConstraintBase";
        public virtual IEnumerable<string> RenderUiConstraintValue(AggregateMember.ValueMember vm) {
            yield break;
        }

        public string GetUiDisplayName() => "ファイル";
        public string GetHelpText() => $$"""
            添付ファイル型。
            この型のインスタンスは、アップロードする時のみ、そのファイルのバイナリを保持します。
            それ以外の時は、アップロードされたファイルのメタデータのみを保持します。
            """;


        // 添付ファイルに対する検索はできない。
        public string GetSearchConditionCSharpType(AggregateMember.ValueMember vm) => "string";
        public string GetSearchConditionTypeScriptType(AggregateMember.ValueMember vm) => "undefined";
        public string RenderFilteringStatement(AggregateMember.ValueMember member, string query, string searchCondition, E_SearchConditionObject searchConditionObject, E_SearchQueryObject searchQueryObject) {
            var pathFromSearchCondition = searchConditionObject == E_SearchConditionObject.SearchCondition
              ? member.Declared.GetFullPathAsSearchConditionFilter(E_CsTs.CSharp)
              : member.Declared.GetFullPathAsRefSearchConditionFilter(E_CsTs.CSharp);
            var fullpathNullable = $"{searchCondition}.{pathFromSearchCondition.Join("?.")}";
            var fullpathNotNull = $"{searchCondition}.{pathFromSearchCondition.Join(".")}";
            var whereFullpath = searchQueryObject == E_SearchQueryObject.SearchResult
                ? member.GetFullPathAsSearchResult(E_CsTs.CSharp, out var isArray)
                : member.GetFullPathAsDbEntity(E_CsTs.CSharp, out isArray);

            return $$"""
                // {{member.DisplayName}} に対する検索はサポ－トされていません。
                """;
        }
        public string RenderSearchConditionVFormBody(AggregateMember.ValueMember vm, FormUIRenderingContext ctx) {
            var attrs = new List<string>();
            var fullpath = ctx.GetReactHookFormFieldPath(vm.Declared).Join(".");
            attrs.Add($"{{...{ctx.Register}(`{fullpath}`)}}");

            if (vm.Options.UiWidth != null) {
                var rem = vm.Options.UiWidth.GetCssValue();
                attrs.Add($"className=\"min-w-[{rem}]\"");
                attrs.Add($"inputClassName=\"max-w-[{rem}] min-w-[{rem}]\"");
            }

            return $$"""
                <Input.Word {{attrs.Join(" ")}}/>
                """;
        }


        public string RenderSingleViewVFormBody(AggregateMember.ValueMember vm, FormUIRenderingContext ctx) {
            var attrs = new List<string>();
            var fullpath = ctx.GetReactHookFormFieldPath(vm.Declared).Join(".");
            attrs.Add($"{{...{ctx.Register}(`{fullpath}`)}}");
            attrs.Add(ctx.RenderReadOnlyStatement(vm.Declared));

            if (vm.Options.UiWidth != null) {
                var rem = vm.Options.UiWidth.GetCssValue();
                attrs.Add($"className=\"min-w-[{rem}]\"");
                attrs.Add($"inputClassName=\"max-w-[{rem}] min-w-[{rem}]\"");
            }

            return $$"""
                <Input.FileAttachmentView {{attrs.Join(" ")}}/>
                {{ctx.RenderErrorMessage(vm)}}
                """;
        }


        // DataTableの列にはファイル名を表示する。
        public string DataTableColumnDefHelperName => "file";

        public WijmoGridColumnSetting GetWijmoGridColumnSetting() {
            return new() {
                DataType = "Cell",
                Format = null,
            };
        }


        // C#とDBとの型変換。DBにはメタデータのJSONを保存する。
        // クラス定義はテンプレートプロジェクトにあるのでそちらを参照。
        void IAggregateMemberType.GenerateCode(CodeRenderingContext context) {
            var dbContext = context.UseSummarizedFile<Parts.WebServer.DbContextClass>();
            dbContext.AddOnModelCreatingPropConverter(GetCSharpTypeName(), "GetFileAttachmentEFCoreValueConverter");

            var util = context.UseSummarizedFile<Parts.WebServer.UtilityClass>();
            util.AddJsonConverter(new Parts.WebServer.UtilityClass.CustomJsonConverter {
                ConverterClassName = "FileAttachmentMetadata.JsonValueConverter",
                ConverterClassDeclaring = string.Empty,
            });

            context.UseSummarizedFile<Parts.Configure>().AddMethod($$"""
                /// <summary>
                /// <see cref="{{GetCSharpTypeName()}}"/> クラスのプロパティがDBとC#の間で変換されるときの処理を定義するクラスを返します。
                /// </summary>
                public virtual Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter GetFileAttachmentEFCoreValueConverter() {
                    return new EFCoreFileAttachmentConverter();
                }
                """);
        }
    }
}
