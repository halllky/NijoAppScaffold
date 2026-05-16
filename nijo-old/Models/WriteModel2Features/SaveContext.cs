using Nijo.Core;
using Nijo.Models.ReadModel2Features;
using Nijo.Parts.BothOfClientAndServer;
using Nijo.Parts.WebServer;
using Nijo.Util.CodeGenerating;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Models.WriteModel2Features {
    /// <summary>
    /// 保存処理関連のコンテキスト引数
    /// </summary>
    internal class SaveContext : ISummarizedFile {

        private readonly List<GraphNode<Aggregate>> _writeModels = new();
        internal void AddWriteModel(GraphNode<Aggregate> rootAggregate) {
            _writeModels.Add(rootAggregate);
        }

        /// <summary>
        /// 一括更新処理全体を通しての状態を持つクラス。
        /// カスタマイズ処理の中でこのクラスを直に参照することはない想定。
        /// </summary>
        internal const string STATE_CLASS_NAME = "BatchUpdateState";
        /// <summary>
        /// データ作成・更新・削除前イベント引数
        /// </summary>
        internal const string BEFORE_SAVE = "BeforeSaveEventArgs";
        /// <summary>
        /// データ作成・更新・削除の後、トランザクションのコミット前に実行されるイベントの引数
        /// </summary>
        internal const string AFTER_SAVE_EVENT_ARGS = "AfterSaveEventArgs";
        /// <summary>
        /// 一括更新処理の細かい挙動を呼び出し元で指定できるようにするためのオプション
        /// </summary>
        internal const string SAVE_OPTIONS = "SaveOptions";

        void ISummarizedFile.OnEndGenerating(CodeRenderingContext context) {
            context.CoreLibrary.UtilDir(utilDir => {
                utilDir.Generate(Render());
            });
        }

        internal SourceFile Render() => new SourceFile {
            FileName = "SaveContext.cs",
            RenderContent = context => {
                var saveCommands = _writeModels.Select(agg => new {
                    CreateCommand = new DataClassForSave(agg, DataClassForSave.E_Type.Create),
                    SaveCommand = new DataClassForSave(agg, DataClassForSave.E_Type.UpdateOrDelete),
                });

                return $$"""
                    using System.Text.Json.Nodes;

                    namespace {{context.Config.RootNamespace}};

                    /// <summary>
                    /// 一括更新処理の細かい挙動を呼び出し元で指定できるようにするためのオプション
                    /// </summary>
                    public partial class {{SAVE_OPTIONS}} {
                        /// <summary>
                        /// trueの場合、 <see cref="AddConfirm" /> による警告があっても更新処理が続行されます。
                        /// 画面側で警告に対して「はい(Yes)」が選択されたあとのリクエストではこの値がtrueになります。
                        /// </summary>
                        public required bool IgnoreConfirm { get; init; }
                    }

                    /// <summary>
                    /// 一括更新処理のデータ1件分のコンテキスト引数。エラーメッセージや確認メッセージなどを書きやすくするためのもの。
                    /// </summary>
                    /// <typeparam name="TMessage">ユーザーに通知するメッセージデータの構造体</typeparam>
                    public partial class {{BEFORE_SAVE}}<TMessage> {
                        public {{BEFORE_SAVE}}({{PresentationContext.INTERFACE_NAME}} state, TMessage messages) {
                            _state = state;
                            Messages = messages;
                        }
                        private readonly {{PresentationContext.INTERFACE_NAME}} _state;

                        /// <inheritdoc cref="{{SAVE_OPTIONS}}" />
                        public {{SAVE_OPTIONS}} Options => _state.Options;

                        /// <summary>ユーザーに通知するメッセージデータ</summary>
                        public TMessage Messages { get; }

                        /// <summary>
                        /// <para>
                        /// 更新処理を実行してもよいかどうかをユーザーに問いかけるメッセージを追加します。
                        /// </para>
                        /// <para>
                        /// ボタンの意味を統一してユーザーが混乱しないようにするため、
                        /// 「はい(Yes)」を選択したときに処理が続行され、
                        /// 「いいえ(No)」を選択したときに処理が中断されるような文言にしてください。
                        /// </para>
                        /// <para>
                        /// <see cref="IgnoreConfirm"/> がfalseのリクエストで何らかのコンファームが発生した場合、
                        /// 更新処理は中断されます。
                        /// </para>
                        /// </summary>
                        public void AddConfirm(string message) {
                            _state.AddConfirm(message);
                        }

                        /// <summary>
                        /// 警告が1件以上あるかどうかを返します。
                        /// </summary>
                        public bool HasConfirm() {
                            return _state.HasConfirm();
                        }
                    }

                    /// <summary>
                    /// 更新後イベント引数
                    /// </summary>
                    public partial class {{AFTER_SAVE_EVENT_ARGS}} {
                        public {{AFTER_SAVE_EVENT_ARGS}}({{PresentationContext.INTERFACE_NAME}} batchUpdateState) {
                            _batchUpdateState = batchUpdateState;
                        }
                        protected readonly {{PresentationContext.INTERFACE_NAME}} _batchUpdateState;
                    }
                    """;
            },
        };
    }
}
