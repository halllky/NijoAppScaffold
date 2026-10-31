using Nijo.CodeGenerating;
using Nijo.DataStructures.IsGenericLookupTable;
using Nijo.Parts.Common;
using Nijo.SchemaParsing;
using Nijo.Util.DotnetEx;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;
using Nijo.DataStructures.Aggregates;
using Nijo.DataStructures;
using Nijo.DataStructures.ValueMemberTypes;

namespace Nijo.DataStructures.IsStored;

/// <summary>
/// 永続化されるデータ構造の項目1個単位の値の妥当性チェック（文字数、必須チェックなど）を行うための基底クラス。
/// チェック処理は新規登録コマンドと更新コマンドのメソッドとしてレンダリングされる。
/// </summary>
public abstract class ValidatorBase {
    /// <summary>
    /// 永続化されるデータ構造の登録・更新時に実行されるバリデーターを列挙します。
    /// </summary>
    internal static IEnumerable<ValidatorBase> EnumerateAll(CodeRenderingContext ctx) {
        yield return new ValidateRequired();
        yield return new ValidateGenericLookupExists();
        yield return new ValidateMaxLength();
        yield return new ValidateDigitsAndScales();

        var customValidationAttributes = ctx.Schema.CustomAttributes
            .Where(attr => attr.IsValidation
                        && !string.IsNullOrWhiteSpace(attr.UniqueId)
                        && !string.IsNullOrWhiteSpace(attr.PhysicalName));
        foreach (var attr in customValidationAttributes) {
            yield return new ValidateCustom(attr);
        }
    }

    /// <summary>
    /// XMLコメント内にレンダリングされるこのチェック処理の名前
    /// </summary>
    public abstract string CommentName { get; }
    /// <summary>
    /// レンダリングされるメソッド名
    /// </summary>
    public abstract string MethodName { get; }
    /// <summary>
    /// エラーメッセージの識別子。<see cref="MsgFactory.AddMessage"/> に使用されます。
    /// </summary>
    public abstract string MsgId { get; }
    /// <summary>
    /// エラーメッセージのテンプレート。<see cref="MsgFactory.AddMessage"/> に使用されます。
    /// </summary>
    public abstract string MsgTemplate { get; }

    #region バリデーション式本体
    /// <summary>
    /// メソッドの先頭で何かしらの変数を宣言したりするのに使用
    /// </summary>
    protected virtual IEnumerable<string> RenderMethodHead() {
        yield break;
    }

    /// <summary>
    /// 項目単位の妥当性チェック処理のif文の中の式を表す情報を返します。
    /// null を返した場合、当該項目はチェック対象となりません。
    /// </summary>
    /// <param name="vm">チェック対象の項目</param>
    /// <param name="prop">コマンドから該当のプロパティまでの経路情報をもったオブジェクト</param>
    protected abstract ValidateStatement? GetIfStatement(ValueMember vm, IInstanceProperty prop, CodeRenderingContext ctx);

    /// <summary>
    /// 項目単位の妥当性チェック処理を複数返す場合に使用します。
    /// デフォルトでは <see cref="GetIfStatement(ValueMember, IInstanceProperty, CodeRenderingContext)"/> の結果のみを返します。
    /// </summary>
    protected virtual IEnumerable<ValidateStatement> GetIfStatements(ValueMember vm, IInstanceProperty prop, CodeRenderingContext ctx) {
        var stmt = GetIfStatement(vm, prop, ctx);
        if (stmt != null) yield return stmt;
    }

    /// <summary>
    /// 参照項目の妥当性チェック処理のif文の中の式を表す情報を返します。
    /// null を返した場合、当該項目はチェック対象となりません。
    /// </summary>
    /// <param name="refTo">チェック対象の参照項目</param>
    /// <param name="keyProps">参照先のキーを構成する値のプロパティのリスト</param>
    protected virtual ValidateStatement? GetIfStatement(RefToMember refTo, IEnumerable<IInstanceProperty> keyProps, CodeRenderingContext ctx) => null;

    /// <summary>
    /// 参照項目の妥当性チェック処理を複数返す場合に使用します。
    /// デフォルトでは <see cref="GetIfStatement(RefToMember, IEnumerable{IInstanceProperty}, CodeRenderingContext)"/> の結果のみを返します。
    /// </summary>
    protected virtual IEnumerable<ValidateStatement> GetIfStatements(RefToMember refTo, IEnumerable<IInstanceProperty> keyProps, CodeRenderingContext ctx) {
        var stmt = GetIfStatement(refTo, keyProps, ctx);
        if (stmt != null) yield return stmt;
    }

    /// <summary>
    /// コマンドの値はドメイン型なので、プリミティブ型の値として扱うための式を返します。
    /// </summary>
    /// <param name="prop">値のプロパティ</param>
    /// <param name="nullableSeparator">プロパティのパスの結合に使われる文字列</param>
    protected static string RenderPrimitiveValue(IInstanceProperty prop, string nullableSeparator = ".") {
        var path = prop.GetJoinedPathFromInstance(E_CsTs.CSharp, nullableSeparator);
        var cast = ((IInstanceValuePropertyMetadata)prop.Metadata).Type.RenderCastToPrimitiveType();
        return cast == string.Empty ? path : $"({cast}{path})";
    }

    /// <summary>
    /// 妥当性チェックの条件を表す情報
    /// </summary>
    protected class ValidateStatement {
        /// <summary>
        /// if () の中の式。 true の場合にエラーとなるようなソースコードを指定します。
        /// </summary>
        public string? If { get; init; }
        /// <summary>
        /// <see cref="If"/> をメッセージのアクセサ（例: messages.Foo[0].Bar）を受け取って動的に生成する場合に使用。
        /// </summary>
        public Func<string, string>? IfFactory { get; init; }
        /// <summary>
        /// エラーメッセージのレンダリング。
        /// <see cref="Parts.Common.MsgFactory.MSG"/>.メッセージID("パラメータ") の形で返してください。
        /// </summary>
        public string? RenderErrorMessage { get; init; }
        /// <summary>
        /// <see cref="RenderErrorMessage"/> をメッセージのアクセサ（例: messages.Foo[0].Bar）を受け取って動的に生成する場合に使用。
        /// </summary>
        public Func<string, string>? RenderErrorMessageFactory { get; init; }
    }
    #endregion バリデーション式本体

    /// <summary>
    /// コマンド自身の値を検証してメッセージコンテナに追記するメソッドをレンダリングします。
    /// </summary>
    internal string RenderDeclaring(SaveCommand rootCommand, CodeRenderingContext ctx) {
        var messages = new SaveCommandMessageContainer(rootCommand.Aggregate);
        var body = RenderAggregate(new Variable("this", rootCommand), ["messages"]).ToArray();

        return $$"""
            /// <summary>
            /// {{CommentName}}。違反する項目があった場合はその旨が引数のオブジェクト内に追記されます。
            /// </summary>
            private void {{MethodName}}({{messages.CsClassName}} messages) {
            {{If(body.Length == 0, () => $$"""
                // 対象項目なし
            """).Else(() => $$"""
            {{RenderMethodHead().SelectTextTemplate(source => $$"""
                {{WithIndent(source)}}
            """)}}
                {{WithIndent(body)}}
            """)}}
            }
            """;

        IEnumerable<string> RenderAggregate(IInstancePropertyOwner currentInstance, string[] ownerPath) {
            foreach (var prop in currentInstance.CreateProperties()) {
                if (prop.Metadata is SaveCommand.SaveCommandValueMember valueMember) {
                    foreach (var ifStatement in GetIfStatements(valueMember.Member, prop, ctx)) {
                        yield return RenderIfStatement(ifStatement, valueMember.Member.DisplayName, [.. ownerPath, valueMember.PhysicalName]);
                    }

                } else if (prop.Metadata is SaveCommand.SaveCommandRefMember refMember) {
                    var keyProps = ((InstanceStructureProperty)prop)
                        .Create1To1PropertiesRecursively()
                        .OfType<InstanceValueProperty>()
                        .ToArray();
                    foreach (var ifStatement in GetIfStatements(refMember.Member, keyProps, ctx)) {
                        yield return RenderIfStatement(ifStatement, refMember.Member.DisplayName, [.. ownerPath, refMember.Member.PhysicalName]);
                    }

                } else if (prop.Metadata is SaveCommand.SaveCommandChildMember childMember) {
                    var childProp = (InstanceStructureProperty)prop;
                    var childBody = RenderAggregate(childProp, [.. ownerPath, childMember.PhysicalName]).ToArray();
                    if (childBody.Length == 0) continue;

                    yield return $$"""
                        if ({{childProp.GetJoinedPathFromInstance(E_CsTs.CSharp)}} != null) {
                            {{WithIndent(childBody)}}
                        }
                        """;

                } else if (prop.Metadata is SaveCommand.SaveCommandChildrenMember childrenMember) {
                    var i = childrenMember.Aggregate.GetLoopVarName("i");
                    var loopItem = new Variable(childrenMember.Aggregate.GetLoopVarName("item"), childrenMember);
                    var childrenPath = prop.GetJoinedPathFromInstance(E_CsTs.CSharp);
                    var childBody = RenderAggregate(loopItem, [.. ownerPath, $"{childrenMember.PhysicalName}[{i}]"]).ToArray();
                    if (childBody.Length == 0) continue;

                    yield return $$"""
                        for (var {{i}} = 0; {{i}} < {{childrenPath}}.Count; {{i}}++) {
                            var {{loopItem.Name}} = {{childrenPath}}[{{i}}];

                            {{WithIndent(childBody)}}
                        }
                        """;
                }
            }
        }

        string RenderIfStatement(ValidateStatement ifStatement, string displayName, string[] errorPath) {
            var messageAccessor = errorPath.Join(".");
            var ifExpression = ifStatement.IfFactory?.Invoke(messageAccessor)
                ?? ifStatement.If
                ?? throw new InvalidOperationException("if文の条件が設定されていません。");
            var renderErrorMessage = ifStatement.RenderErrorMessageFactory?.Invoke(messageAccessor)
                ?? ifStatement.RenderErrorMessage
                ?? throw new InvalidOperationException("エラーメッセージが設定されていません。");

            return $$"""
                // {{displayName}}
                if ({{WithIndent(ifExpression, "    ")}}) {
                    {{messageAccessor}}.AddError({{renderErrorMessage}});
                }
                """;
        }
    }
}
