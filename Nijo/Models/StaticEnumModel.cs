using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Parts.Common;
using Nijo.SchemaParsing;
using Nijo.Models.StaticEnumModelModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Nijo.Models {
    /// <summary>
    /// 静的区分値。値がソースコード上にハードコードされる区分値。
    /// C#はenumとして、TypeScriptはリテラル型として表れる。
    /// </summary>
    internal static class StaticEnumModel {

        /// <summary>
        /// ルート集約1個と対応するソースコードを生成します。
        /// </summary>
        internal static void Render(CodeRenderingContext ctx, RootAggregate rootAggregate) {
            var enumFile = ctx.Use<EnumFile>();
            var parser = new EnumDefParser(((ISchemaPathNode)rootAggregate).XElement, ctx.SchemaParser);


            // データ型: enum, リテラル型定義
            var staticEnum = new StaticEnumDef(parser, rootAggregate);
            enumFile.Register(staticEnum);
            enumFile.AddCSharpSource(staticEnum.RenderCSharp());
            enumFile.AddTypeScriptSource(staticEnum.RenderTypeScript());

            // データ型: 検索条件オブジェクト
            var searchCondition = new StaticEnumSearchCondition(staticEnum, parser);
            enumFile.AddCSharpSource(searchCondition.RenderCSharp());
            enumFile.AddTypeScriptSource(searchCondition.RenderTypeScript());
        }

    }
}
