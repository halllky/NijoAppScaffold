using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Models.DataModelModules;
using Nijo.Parts.Common;
using System;

namespace Nijo.Models.DataModelModules {
    /// <summary>
    /// 文字種チェック
    /// </summary>
    internal static class ValidateCharacterType {

        internal const string METHOD_NAME = "ValidateCharacterType";

        internal static string Render(RootAggregate rootAggregate, CodeRenderingContext ctx) {
            var efCoreEntity = new EFCoreEntity(rootAggregate);
            var messages = new SaveCommandMessageContainer(rootAggregate);

            return $$"""
                /// <summary>
                /// 文字列系項目の文字種チェック。違反する項目があった場合はその旨が第2引数のオブジェクト内に追記されます。
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
