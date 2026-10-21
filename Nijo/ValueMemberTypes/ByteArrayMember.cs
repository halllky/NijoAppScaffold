using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.SchemaParsing;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Nijo.ValueMemberTypes;

/// <summary>
/// バイト配列型
/// </summary>
internal class ByteArrayMember : IValueMemberType {
    public string TypePhysicalName => "ByteArray";
    public string SchemaTypeName => "bytearray";
    public string CsDomainTypeName => "byte[]";
    public string CsPrimitiveTypeName => "byte[]";
    public string TsTypeName => "/* バイト配列型のメンバーはクライアント側ソースにレンダリングされることはない想定 */";
    public string DisplayName => "バイト配列";

    string IValueMemberType.RenderSpecificationMarkdown() {
        return $$"""
            バイナリデータを格納する型です。
            ファイル、画像、暗号化データなどのバイナリ情報に適しています。
            IsStored 属性のみが指定されたデータ構造でのみ定義できます。
            """;
    }

    void IValueMemberType.Validate(XElement element, SchemaParseContext context, Action<XElement, string> addError) {
        // バイト配列を画面や帳票で用いることはできないため、永続化されるが検索されないデータ構造にしか定義できない
        if (!element.IsStored()) {
            addError(element, $"バイト配列を画面や帳票で用いることはできないため、{SchemaTypeName}は{BasicNodeOptions.IsStored.AttributeName}属性が指定されたデータ構造にしか定義できません。");

        } else if (element.IsQueryable()) {
            addError(element, $"バイト配列を画面や帳票で用いることはできないため、{BasicNodeOptions.IsQueryable.AttributeName}属性が指定されたデータ構造に{SchemaTypeName}を使用することはできません。");
        }
    }

    ValueMemberSearchBehavior? IValueMemberType.SearchBehavior => null; // バイト配列に対する検索は提供しない

    void IValueMemberType.RegisterDependencies(CodeRenderingContext ctx) {
        // 特になし
    }

    string IValueMemberType.RenderCreateDummyDataValueBody(CodeRenderingContext ctx) {
        return $$"""
            // ダミーデータとして8バイトのランダムなバイト配列を生成
            var dummyBytes = new byte[8];
            if (member.IsKey) {
                // キーの場合はシーケンス値を含む一意なデータを生成
                var seq = context.GetNextSequence();
                BitConverter.GetBytes(seq).CopyTo(dummyBytes, 0);
                context.Random.NextBytes(new Span<byte>(dummyBytes, 4, 4));
            } else {
                context.Random.NextBytes(dummyBytes);
            }
            return dummyBytes;
            """;
    }

    void IValueMemberType.RenderStaticSources(CodeRenderingContext ctx) {
        // 特になし
    }
}
