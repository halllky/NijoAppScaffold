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

    /// <summary>
    /// 項目単位の妥当性チェック処理のif文の中の式を表す情報を返します。
    /// null を返した場合、当該項目はチェック対象となりません。
    /// </summary>
    /// <param name="vm">チェック対象の項目</param>
    /// <param name="prop">メソッド引数から該当のプロパティまでの経路情報をもったオブジェクト</param>
    protected abstract ValidateStatement? GetIfStatement(ValueMember vm, IInstanceProperty prop, CodeRenderingContext ctx);

    /// <summary>
    /// 妥当性チェックの条件を表す情報
    /// </summary>
    protected class ValidateStatement {
        /// <summary>
        /// if () の中の式。 true の場合にエラーとなるようなソースコードを指定します。
        /// </summary>
        public required string If { get; init; }
        /// <summary>
        /// エラーメッセージのレンダリング。
        /// <see cref="Parts.Common.MsgFactory.MSG"/>.メッセージID("パラメータ") の形で返してください。
        /// </summary>
        public required string RenderErrorMessage { get; init; }
    }

    /// <summary>
    /// EFCore エンティティとメッセージコンテナを受け取り、妥当性チェックを行うメソッドをレンダリングします。
    /// </summary>
    internal string Render(RootAggregate rootAggregate, CodeRenderingContext ctx) {
        var efCoreEntity = new EFCoreEntity(rootAggregate);
        var messages = new SaveCommandMessageContainer(rootAggregate);

        var arg = new Variable("dbEntity", efCoreEntity);

        return $$"""
            /// <summary>
            /// {{CommentName}}。違反する項目があった場合はその旨が第2引数のオブジェクト内に追記されます。
            /// 1件以上エラーがあった場合はfalseを返します。
            /// </summary>
            protected virtual bool {{MethodName}}({{efCoreEntity.CsClassName}} {{arg.Name}}, {{messages.InterfaceName}} messages) {
                var isValid = true;

                {{WithIndent(RenderAggregate(efCoreEntity, arg, ["messages"]), "    ")}}

                return isValid;
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
            foreach (var member in currentEntity.Aggregate.GetMembers()) {
                if (member is not ValueMember vm) continue;

                // 判定対象の項目でなければスキップ
                var prop = ownColumnProps[vm];
                var ifStatement = GetIfStatement(vm, prop, ctx);
                if (ifStatement == null) continue;

                string[] errorPath = [.. ownerPath, prop.Metadata.GetPropertyName(E_CsTs.CSharp)];

                yield return $$"""
                    // {{vm.DisplayName}}
                    if ({{WithIndent(ifStatement.If, "    ")}}) {
                        {{errorPath.Join(".")}}.AddError({{ifStatement.RenderErrorMessage}});
                        isValid = false;
                    }
                    """;
            }

            // 子孫エンティティ
            var childProps = props
                .Where(p => p.Metadata is NavigationProperty.PrincipalOrRelevant)
                .ToDictionary(p => ((NavigationProperty.PrincipalOrRelevant)p.Metadata).OtherSide, p => p);
            foreach (var nav in currentEntity.GetNavigationProperties()) {
                if (nav is not NavigationProperty.NavigationOfParentChild nop) continue; // 外部参照のナビゲーションを除外
                if (nop.Principal.ThisSide != currentEntity.Aggregate) continue; // 子から親へのナビゲーションを除外

                if (nop.Relevant.ThisSide is ChildAggregate child) {
                    var childEntity = new EFCoreEntity(child);
                    var childNav = childProps[child];
                    string[] childErrorPath = [.. ownerPath, childNav.Metadata.GetPropertyName(E_CsTs.CSharp)];

                    var body = RenderAggregate(childEntity, (IInstancePropertyOwner)childNav, childErrorPath).ToArray();
                    if (body.Length == 0) continue;

                    yield return $$"""
                        if ({{childNav.GetJoinedPathFromInstance(E_CsTs.CSharp)}} != null) {
                            {{WithIndent(body, "    ")}}
                        }
                        """;

                } else if (nop.Relevant.ThisSide is ChildrenAggregate children) {
                    var childrenEntity = new EFCoreEntity(children);
                    var childrenNav = childProps[children];
                    var i = children.GetLoopVarName("i");
                    var loopItem = new Variable(children.GetLoopVarName("item"), childrenEntity);
                    string[] childErrorPath = [.. ownerPath, $"{childrenNav.Metadata.GetPropertyName(E_CsTs.CSharp)}[{i}]"];

                    var body = RenderAggregate(childrenEntity, loopItem, childErrorPath).ToArray();
                    if (body.Length == 0) continue;

                    yield return $$"""
                        for (var {{i}} = 0; {{i}} < {{childrenNav.GetJoinedPathFromInstance(E_CsTs.CSharp)}}.Count; {{i}}++) {
                            var {{loopItem.Name}} = {{childrenNav.GetJoinedPathFromInstance(E_CsTs.CSharp)}}.ElementAt({{i}});

                            {{WithIndent(body, "    ")}}
                        }
                        """;

                } else {
                    throw new InvalidOperationException("Child か Children しか来ないはず");
                }
            }
        }
    }
}
