using Nijo.Core;
using Nijo.Models.CommandModelFeatures;
using Nijo.Models.ReadModel2Features;
using Nijo.Util.CodeGenerating;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Models {
    /// <summary>
    /// コマンド。
    /// ユーザーがパラメータを指定し、サーバー側で同期的に実行される処理。
    /// スキーマ定義で設定されたデータ構造はコマンドのパラメータを表す。
    /// </summary>
    internal class CommandModel : IModel {
        void IModel.GenerateCode(CodeRenderingContext context, GraphNode<Aggregate> rootAggregate) {
            var aggregateFile = context.CoreLibrary.UseAggregateFile(rootAggregate);

            // データ型: パラメータ型定義
            var parameter = new CommandParameter(rootAggregate);
            aggregateFile.DataClassDeclaring.Add(parameter.RenderCSharpDeclaring(context));
            aggregateFile.TypeScriptFile.Add(parameter.RenderTsDeclaring(context));

            // データ型: エラーメッセージ
            aggregateFile.DataClassDeclaring.Add(parameter.RenderCSharpMessageClassDeclaring(context));

            // 処理: Reactフック、Webエンドポイント、本処理抽象メソッド
            var commandMethod = new CommandMethod(rootAggregate);
            aggregateFile.TypeScriptFile.Add(commandMethod.RenderHook(context));
            aggregateFile.ControllerActions.Add(commandMethod.RenderController(context));
            aggregateFile.AppServiceMethods.Add(commandMethod.RenderAbstractMethod(context));

            // 列挙体: ステップ
            context.CoreLibrary.Enums.Add(commandMethod.RenderStepEnum());

            // 処理: クライアント側新規オブジェクト作成関数
            aggregateFile.TypeScriptFile.Add(parameter.RenderTsNewObjectFunction(context));

            // 権限
            context.UseSummarizedFile<AuthorizedAction>().Regeister(rootAggregate);

            // UI: カスタマイズ用のユーティリティ
            context.UseSummarizedFile<DisplayDataTypeList>().AddCommand(rootAggregate);
        }

        void IModel.GenerateCode(CodeRenderingContext context) {
            // データ型: ステップ変更時イベント引数、実行時イベント引数
            context.CoreLibrary.UtilDir(dir => {
                //dir.Generate(CommandMethod.RenderStepChangeEventArgs()); // 登場しないのでコメントアウト
            });

            // 共通Controllerクラス
            context.WebApiProject.ControllerDir(dir => {
                dir.Generate(CommandController.Render());
            });

            /// 処理結果の型のレンダリングは <see cref="CommandResult"/> 側で行なっています

        }

        IEnumerable<string> IModel.ValidateAggregate(GraphNode<Aggregate> rootAggregate) {
            var existsInvalidStep = false;
            foreach (var aggregate in rootAggregate.EnumerateThisAndDescendants()) {

                // ステップ属性をつけることができるのはルート集約の直下のみ
                if (aggregate.Item.Options.Step != null
                    && (aggregate.GetParent()?.Initial.IsRoot() != true
                    || !aggregate.IsChildMember())) {
                    yield return $"{aggregate.Item.DisplayName}: step属性を定義できるのはルート集約の直下の{nameof(AggregateMember.Child)}集約のみです。";
                    existsInvalidStep = true;
                }

                foreach (var member in aggregate.GetMembers()) {

                    // キー指定不可
                    if (member is AggregateMember.ValueMember vm && vm.IsKey
                        || member is AggregateMember.Ref @ref && @ref.Relation.IsPrimary()) {
                        yield return $"{aggregate.Item.DisplayName}.{member.MemberName}: {nameof(CommandModel)}のメンバーをキーに指定することはできません。";
                    }
                }
            }

            // ステップ属性の有無は混在不可能
            if (!existsInvalidStep) {
                var ownMembers = rootAggregate
                    .GetMembers()
                    .Where(m => m.DeclaringAggregate == rootAggregate);
                var steps = ownMembers
                    .Where(m => m is AggregateMember.RelationMember rel
                             && rel.MemberAggregate.Item.Options.Step != null)
                    .ToArray();
                var notSteps = ownMembers
                    .Where(m => m is not AggregateMember.RelationMember rel
                             || rel.MemberAggregate.Item.Options.Step == null)
                    .ToArray();
                if (steps.Length > 0 && notSteps.Length > 0) {
                    yield return $"{rootAggregate.Item.DisplayName}: step属性をつける場合はルート集約の「直下」の全ての要素にstep属性をつける必要があります。";
                }
            }
        }
    }
}
