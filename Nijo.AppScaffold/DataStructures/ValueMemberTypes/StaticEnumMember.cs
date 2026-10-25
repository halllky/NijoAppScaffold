using Nijo.CodeGenerating;
using Nijo.SchemaParsing;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Nijo.StaticEnums;

namespace Nijo.DataStructures.ValueMemberTypes {
    /// <summary>
    /// 列挙体型のメンバー
    /// </summary>
    internal class StaticEnumMember : IValueMemberType {

        internal StaticEnumMember(XElement el, SchemaParseContext ctx) {
            _xElement = el;
            _staticEnum = new StaticEnumRoot(el, ctx);
        }

        private readonly XElement _xElement;
        private readonly StaticEnumRoot _staticEnum;

        public string TypePhysicalName => _xElement.Name.LocalName;
        public string SchemaTypeName => _xElement.Name.LocalName;
        public string CsDomainTypeName => _staticEnum.CsEnumName;
        public string CsPrimitiveTypeName => _staticEnum.CsEnumName;
        public string TsTypeName => $"EnumDefs.{_staticEnum.TsTypeName}";
        public string DisplayName => "列挙型";

        string IValueMemberType.RenderSpecificationMarkdown() {
            return $$"""
                事前に定義された選択肢から値を選択する型です。
                ステータス、カテゴリ、区分などの固定選択肢データに適しています。
                検索時は複数の選択肢を同時に指定できます。

                予め nijo.xml の `{{E_Section.StaticEnums.ToXmlElementName()}}` セクションに区分の種類を定義しておく必要があります。
                """;
        }

        ValueMemberSearchBehavior? IValueMemberType.SearchBehavior => new() {
            FilterCsTypeName = _staticEnum.CsSearchConditionClassName,
            FilterTsTypeName = _staticEnum.RenderTsSearchConditionType(),
            RenderTsNewObjectFunctionValue = () => _staticEnum.RenderTsSearchConditionInitializer(),
            RenderFiltering = ctx => {
                var query = ctx.Query.Root.Name;
                var fullpathNullable = ctx.SearchCondition.GetJoinedPathFromInstance(E_CsTs.CSharp, "?.");
                var fullpathNotNull = ctx.SearchCondition.GetJoinedPathFromInstance(E_CsTs.CSharp, ".");

                var queryFullPath = ctx.Query.GetFlattenArrayPath(E_CsTs.CSharp, out var isMany);
                var queryOwnerFullPath = queryFullPath.SkipLast(1);

                return $$"""
                    if ({{fullpathNullable}}?.{{StaticEnumSearchCondition.ANY_CHECKED}}() == true) {
                        var array = new List<{{CsPrimitiveTypeName}}>();
                    {{_staticEnum.GetValues().Select(v => v.PhysicalName).SelectTextTemplate(physicalName => $$"""
                        if ({{fullpathNotNull}}!.{{physicalName}}) array.Add({{CsPrimitiveTypeName}}.{{physicalName}});
                    """)}}
                    {{If(isMany, () => $$"""
                        {{query}} = {{query}}.Where(x => x.{{queryOwnerFullPath.Join("!.")}}.Any(y => y.{{ctx.Query.Metadata.GetPropertyName(E_CsTs.CSharp)}} != null && array.Contains(y.{{ctx.Query.Metadata.GetPropertyName(E_CsTs.CSharp)}}.Value)));
                    """).Else(() => $$"""
                        {{query}} = {{query}}.Where(x => x.{{queryFullPath.Join("!.")}} != null && array.Contains(x.{{queryFullPath.Join("!.")}}.Value));
                    """)}}
                    }
                    """;
            }
        };

        void IValueMemberType.Validate(XElement element, SchemaParseContext context, Action<XElement, string> addError) {
            // 静的区分の定義の検証は StaticEnums セクションが行う
        }

        void IValueMemberType.RegisterDependencies(CodeRenderingContext ctx) {
            // 特になし
        }
        void IValueMemberType.RenderStaticSources(CodeRenderingContext ctx) {
            // 特になし
        }

        string IValueMemberType.RenderCreateDummyDataValueBody(CodeRenderingContext ctx) {
            var count = _staticEnum.GetValues().Count();

            if (count == 0) {
                return $$"""
                    return null;
                    """;
            } else {
                return $$"""
                    return member.IsKey && Enum.GetValues<{{_staticEnum.CsEnumName}}>().Length > 0
                        ? Enum.GetValues<{{_staticEnum.CsEnumName}}>()[context.GetNextSequence() % Enum.GetValues<{{_staticEnum.CsEnumName}}>().Length]
                        : Enum.GetValues<{{_staticEnum.CsEnumName}}>().ElementAt(context.Random.Next(0, {{count - 1}}));
                    """;
            }
        }
    }
}
