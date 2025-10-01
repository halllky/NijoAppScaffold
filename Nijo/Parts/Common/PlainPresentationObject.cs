using System;
using System.Collections.Generic;
using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;

namespace Nijo.Parts.Common;

/// <summary>
/// ユーザーが閲覧する画面上に登場するオブジェクトのうち、
/// スキーマ定義のデータ構造そのままのデータ構造がレンダリングされるもの。
/// </summary>
internal abstract class PlainPresentationObject : IInstancePropertyOwnerMetadata {

    internal abstract string CsClassName { get; }

    internal virtual string GetDescription() {
        return $"{CsClassName}のデータを表示するためのクラス";
    }

    /// <summary>
    /// 直近のメンバーを列挙する
    /// </summary>
    internal abstract IEnumerable<IMember> GetMembers();

    IEnumerable<IInstancePropertyMetadata> IInstancePropertyOwnerMetadata.GetMembers() {
        return GetMembers();
    }

    internal string RenderCsClass(CodeRenderingContext ctx) {
        return $$"""
            /// <summary>
            {{GetDescription().Split(Environment.NewLine).SelectTextTemplate(line => $$"""
            /// {{line}}
            """)}}
            /// </summary>
            public partial class {{CsClassName}} {
            {{GetMembers().SelectTextTemplate(member => $$"""
                {{WithIndent(member.RenderDeclaringCSharp(), "    ")}}
            """)}}
            }
            """;
    }

    /// <summary>
    /// <see cref="PlainPresentationObject"/>のメンバー
    /// </summary>
    internal interface IMember : IInstancePropertyMetadata {
        string PhysicalName { get; }
        string GetTypeName(E_CsTs csts);
        string RenderDeclaringCSharp();
        string RenderDeclaringTypeScript();
    }
}