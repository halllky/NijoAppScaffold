using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Models.DataModelModules;
using Nijo.Parts.Common;
using System;

namespace Nijo.Models.DataModelModules {
    /// <summary>
    /// 数値項目の桁数チェック
    /// </summary>
    internal static class ValidateDigitsAndScales {

        internal const string METHOD_NAME = "ValidateDigitsAndScales";

        internal static string Render(RootAggregate rootAggregate, CodeRenderingContext ctx) {
            var efCoreEntity = new Parts.CSharp.EFCoreEntity(rootAggregate);
            var messages = new SaveCommandMessageContainer(rootAggregate);

            return $$"""
                /// <summary>
                /// 数値の桁数チェック処理。空の項目があった場合はその旨が第2引数のオブジェクト内に追記されます。
                /// エラーがあった場合はfalseを返します。
                /// </summary>
                protected virtual bool {{METHOD_NAME}}({{efCoreEntity.CsClassName}} dbEntity, {{messages.InterfaceName}} messages) {
                    // TODO ver.1
                    return true;
                }
                """;
        }
    }
}
