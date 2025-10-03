using Nijo.CodeGenerating;
using Nijo.Parts.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Parts.CSharp {
    /// <summary>
    /// ユーザーとの対話が発生する何らかの処理で、
    /// 1回のリクエスト・レスポンスのサイクルの一連の情報を保持するコンテキスト情報
    /// </summary>
    internal class PresentationContext {

        internal const string INTERFACE = "IPresentationContext";
        internal const string INTERFACE_WITH_RETURN_VALUE = "IPresentationContextWithReturnValue";
        internal const string OPTIONS = "IPresentationContextOptions";

        internal static SourceFile RenderStaticCore(CodeRenderingContext ctx) {
            return new SourceFile {
                FileName = "IPresentationContext.cs",
                Contents = $$"""
                    namespace {{ctx.Config.RootNamespace}};

                    /// <summary>
                    /// ユーザーとの対話が発生する何らかの処理で、
                    /// 1回のリクエスト・レスポンスのサイクルの一連の情報を保持するコンテキスト情報
                    /// </summary>
                    public interface {{INTERFACE}} {

                        /// <inheritdoc cref="{{OPTIONS}}"/>
                        {{OPTIONS}} Options { get; }

                        /// <summary>
                        /// 「～しますがよろしいですか？」などの確認メッセージを追加します。
                        /// </summary>
                        void AddConfirm(string text);

                        /// <summary>
                        /// 「～しますがよろしいですか？」などの確認メッセージが発生しているかどうかを返します。
                        /// </summary>
                        bool HasConfirm();

                        /// <summary>
                        /// このインスタンスを、メッセージコンテナを持つ型にキャストします。
                        /// このインスタンスが持つ情報はすべて引き継がれます。
                        /// </summary>
                        {{INTERFACE}}<TMessage> As<TMessage>() where TMessage : {{MessageContainer.SETTER_INTERFACE}};
                    }

                    /// <inheritdoc cref="{{INTERFACE}}"/>
                    /// <typeparam name="TMessageRoot">パラメータのメッセージ型</typeparam>
                    public interface {{INTERFACE}}<TMessageRoot> : {{INTERFACE}} where TMessageRoot : {{MessageContainer.SETTER_INTERFACE}} {
                        /// <summary>
                        /// パラメータの各値に対するメッセージ。エラーや警告など。
                        /// </summary>
                        TMessageRoot Messages { get; }
                    }

                    /// <inheritdoc cref="{{INTERFACE}}"/>
                    /// <typeparam name="TReturnValue">戻り値の型</typeparam>
                    public interface {{INTERFACE_WITH_RETURN_VALUE}}<TReturnValue> : {{INTERFACE}}
                        where TReturnValue : new() {

                        /// <summary>
                        /// 画面側に返す戻り値を取得または設定します。
                        /// </summary>
                        TReturnValue ReturnValue { get; set; }
                    }

                    /// <inheritdoc cref="{{INTERFACE}}"/>
                    /// <typeparam name="TReturnValue">戻り値の型</typeparam>
                    /// <typeparam name="TMessageRoot">パラメータのメッセージ型</typeparam>
                    public interface {{INTERFACE_WITH_RETURN_VALUE}}<TReturnValue, TMessageRoot> : {{INTERFACE}}<TMessageRoot>
                        where TMessageRoot : {{MessageContainer.SETTER_INTERFACE}}
                        where TReturnValue : new() {

                        /// <summary>
                        /// 画面側に返す戻り値を取得または設定します。
                        /// </summary>
                        TReturnValue ReturnValue { get; set; }
                    }

                    /// <summary>
                    /// <see cref="{{INTERFACE}}"/> のオプション
                    /// </summary>
                    public interface {{OPTIONS}} {
                        /// <summary>
                        /// Confirm（「～しますがよろしいですか？」の確認メッセージ）が発生しても無視するかどうか。
                        /// HTTPリクエストは「～しますがよろしいですか？」に対してOKが選択される前と後で計2回発生するが、
                        /// 1回目はfalse, 2回目はtrueになる。
                        /// </summary>
                        bool IgnoreConfirm { get; }
                    }
                    """,
            };
        }
    }
}
