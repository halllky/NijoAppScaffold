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

                        /// <summary>
                        /// 更新系の処理について、入力チェックのみを行うか、それとも実際の更新処理も行うかを表す。
                        ///
                        /// Webアプリケーションで何らかのデータを更新する場合、
                        /// 1回目のHTTPリクエストでは入力チェックのみを行い、ユーザーに確認メッセージを表示した上で
                        /// 2回目のHTTPリクエストで再度入力チェックしたうえで実際の更新処理を行う、という2段階の処理を行うことが多い。
                        /// このプロパティが true の場合、入力チェックのみを行い、実際の更新処理は行わない。
                        /// </summary>
                        bool ValidationOnly { get; }

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
                    """,
            };
        }
    }
}
