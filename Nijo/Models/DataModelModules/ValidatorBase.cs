using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Parts.Common;
using Nijo.Parts.CSharp;
using Nijo.Util.DotnetEx;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Nijo.Models.DataModelModules;

/// <summary>
/// DataModel の項目1個単位の値の妥当性チェック（文字数、必須チェックなど）を行うための基底クラス。
/// </summary>
public abstract class ValidatorBase {
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

    #region 追加の引数
    /// <summary>
    /// メソッドの第3引数以降がある場合は指定してください。
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerable<AdditionalArgs> GetAdditionalMethodArgs(RootAggregate rootAggregate) {
        yield break;
    }
    protected class AdditionalArgs {
        public required string Type { get; init; }
        public required string Name { get; init; }
        public required string Comment { get; init; }
    }
    #endregion 追加の引数

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
    /// <param name="prop">メソッド引数から該当のプロパティまでの経路情報をもったオブジェクト</param>
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
    /// <param name="fkProps">外部キーを構成するプロパティのリスト</param>
    protected virtual ValidateStatement? GetIfStatement(RefToMember refTo, IEnumerable<IInstanceProperty> fkProps, CodeRenderingContext ctx) => null;

    /// <summary>
    /// 参照項目の妥当性チェック処理を複数返す場合に使用します。
    /// デフォルトでは <see cref="GetIfStatement(RefToMember, IEnumerable{IInstanceProperty}, CodeRenderingContext)"/> の結果のみを返します。
    /// </summary>
    protected virtual IEnumerable<ValidateStatement> GetIfStatements(RefToMember refTo, IEnumerable<IInstanceProperty> fkProps, CodeRenderingContext ctx) {
        var stmt = GetIfStatement(refTo, fkProps, ctx);
        if (stmt != null) yield return stmt;
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
    /// EFCore エンティティとメッセージコンテナを受け取り、妥当性チェックを行うメソッドをレンダリングします。
    /// </summary>
    internal string RenderDeclaring(RootAggregate rootAggregate, CodeRenderingContext ctx) {
        var efCoreEntity = new EFCoreEntity(rootAggregate);
        var messages = new SaveCommandMessageContainer(rootAggregate);

        var arg = new Variable("dbEntity", efCoreEntity);
        var body = RenderAggregate(efCoreEntity, arg, ["messages"]).ToArray();

        var argList = new List<string> {
            $"{efCoreEntity.CsClassName} {arg.Name}",
            $"{messages.InterfaceName} messages",
        };
        argList.AddRange(GetAdditionalMethodArgs(rootAggregate).Select(a => $"{a.Type} {a.Name}"));

        return $$"""
            /// <summary>
            /// {{CommentName}}。違反する項目があった場合はその旨が第2引数のオブジェクト内に追記されます。
            /// </summary>
            protected virtual void {{MethodName}}({{argList.Join(", ")}}) {
            {{If(body.Length == 0, () => $$"""
                // 対象項目なし
            """).Else(() => $$"""
            {{RenderMethodHead().SelectTextTemplate(source => $$"""
                {{WithIndent(source)}}
            """)}}
                {{WithIndent(RenderAggregate(efCoreEntity, arg, ["messages"]))}}
            """)}}
            }
            """;

        IEnumerable<string> RenderAggregate(
            EFCoreEntity currentEntity,
            IInstancePropertyOwner currentInstance,
            string[] ownerPath) {

            var props = currentInstance
                .CreateProperties()
                .ToArray();

            // 自身の属性
            var ownColumnProps = props
                .Where(p => p.Metadata is EFCoreEntity.OwnColumnMember)
                .ToDictionary(p => ((EFCoreEntity.OwnColumnMember)p.Metadata).Member, p => p);
            // 参照先のキー
            var refKeyProps = props
                .Where(p => p.Metadata is EFCoreEntity.RefKeyMember)
                .GroupBy(p => ((EFCoreEntity.RefKeyMember)p.Metadata).RefEntry)
                .ToDictionary(g => g.Key, g => g.ToArray());
            // ナビゲーションプロパティ
            var navProps = props
                .Where(p => p.Metadata is NavigationProperty.PrincipalOrRelevant)
                .ToDictionary(p => ((NavigationProperty.PrincipalOrRelevant)p.Metadata).NavigationProperty, p => p);

            foreach (var member in currentEntity.Aggregate.GetMembers()) {
                if (member is ValueMember vm) {
                    var prop = ownColumnProps[vm];
                    foreach (var ifStatement in GetIfStatements(vm, prop, ctx)) {
                        string[] errorPath = [.. ownerPath, prop.Metadata.GetPropertyName(E_CsTs.CSharp)];
                        var messageAccessor = errorPath.Join(".");
                        var ifExpression = ifStatement.IfFactory?.Invoke(messageAccessor)
                            ?? ifStatement.If
                            ?? throw new InvalidOperationException("if文の条件が設定されていません。");
                        var renderErrorMessage = ifStatement.RenderErrorMessageFactory?.Invoke(messageAccessor)
                            ?? ifStatement.RenderErrorMessage
                            ?? throw new InvalidOperationException("エラーメッセージが設定されていません。");

                        yield return $$"""
                            // {{vm.DisplayName}}
                            if ({{WithIndent(ifExpression, "    ")}}) {
                                {{messageAccessor}}.AddError({{renderErrorMessage}});
                            }
                            """;
                    }

                } else if (member is RefToMember refTo) {
                    if (!refKeyProps.TryGetValue(refTo, out var fkProps)) throw new InvalidOperationException("外部キー項目が見つかりません。");

                    foreach (var ifStatement in GetIfStatements(refTo, fkProps, ctx)) {
                        var navProp = navProps.Values.Single(p =>
                            ((NavigationProperty.PrincipalOrRelevant)p.Metadata).NavigationProperty is NavigationProperty.NavigationOfRef nav &&
                            nav.Relation == refTo);

                        string[] errorPath = [.. ownerPath, navProp.Metadata.GetPropertyName(E_CsTs.CSharp)];
                        var messageAccessor = errorPath.Join(".");
                        var ifExpression = ifStatement.IfFactory?.Invoke(messageAccessor)
                            ?? ifStatement.If
                            ?? throw new InvalidOperationException("if文の条件が設定されていません。");
                        var renderErrorMessage = ifStatement.RenderErrorMessageFactory?.Invoke(messageAccessor)
                            ?? ifStatement.RenderErrorMessage
                            ?? throw new InvalidOperationException("エラーメッセージが設定されていません。");

                        yield return $$"""
                            // {{refTo.DisplayName}}
                            if ({{WithIndent(ifExpression, "    ")}}) {
                                {{messageAccessor}}.AddError({{renderErrorMessage}});
                            }
                            """;
                    }
                }
            }

            // 子孫エンティティ
            var childProps = props
                .Where(p => p.Metadata is NavigationProperty.PrincipalOrRelevant)
                .ToDictionary(p => ((NavigationProperty.PrincipalOrRelevant)p.Metadata).OtherSide.ToMappingKey(), p => p);
            foreach (var nav in currentEntity.GetNavigationProperties()) {
                if (nav is not NavigationProperty.NavigationOfParentChild nop) continue; // 外部参照のナビゲーションを除外
                if (nop.Principal.ThisSide != currentEntity.Aggregate) continue; // 子から親へのナビゲーションを除外

                if (nop.Relevant.ThisSide is ChildAggregate child) {
                    var childEntity = new EFCoreEntity(child);
                    var childNav = childProps[child.ToMappingKey()];
                    string[] childErrorPath = [.. ownerPath, childNav.Metadata.GetPropertyName(E_CsTs.CSharp)];

                    var body = RenderAggregate(childEntity, (IInstancePropertyOwner)childNav, childErrorPath).ToArray();
                    if (body.Length == 0) continue;

                    yield return $$"""
                        if ({{childNav.GetJoinedPathFromInstance(E_CsTs.CSharp)}} != null) {
                            {{WithIndent(body)}}
                        }
                        """;

                } else if (nop.Relevant.ThisSide is ChildrenAggregate children) {
                    var childrenEntity = new EFCoreEntity(children);
                    var childrenNav = childProps[children.ToMappingKey()];
                    var i = children.GetLoopVarName("i");
                    var loopItem = new Variable(children.GetLoopVarName("item"), childrenEntity);
                    string[] childErrorPath = [.. ownerPath, $"{childrenNav.Metadata.GetPropertyName(E_CsTs.CSharp)}[{i}]"];

                    var body = RenderAggregate(childrenEntity, loopItem, childErrorPath).ToArray();
                    if (body.Length == 0) continue;

                    yield return $$"""
                        for (var {{i}} = 0; {{i}} < {{childrenNav.GetJoinedPathFromInstance(E_CsTs.CSharp)}}.Count; {{i}}++) {
                            var {{loopItem.Name}} = {{childrenNav.GetJoinedPathFromInstance(E_CsTs.CSharp)}}.ElementAt({{i}});

                            {{WithIndent(body)}}
                        }
                        """;

                } else {
                    throw new InvalidOperationException("Child か Children しか来ないはず");
                }
            }
        }
    }

    internal virtual string RenderCaller(object caller, RootAggregate rootAggregate, string dbEntityInstanceName, string messageInstanceName) {
        return $$"""
            {{MethodName}}({{dbEntityInstanceName}}, {{messageInstanceName}})
            """;
    }
}
