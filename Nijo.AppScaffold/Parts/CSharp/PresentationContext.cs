using Nijo.CodeGenerating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Parts.CSharp {
    /// <summary>
    /// クエリやコマンドを呼び出したプレゼンテーション層（Web、バッチ、単体テストなど）が、
    /// 1回の呼び出しについて処理に伝える情報。
    /// 引数の形に沿ったメッセージや戻り値は、処理ごとに形が異なるためここには含めず、処理の引数と戻り値で受け渡す。
    /// </summary>
    internal class PresentationContext {

        internal const string INTERFACE = "IPresentationContext";

        internal static SourceFile RenderStaticCore(CodeRenderingContext ctx) {
            return new SourceFile {
                FileName = "IPresentationContext.cs",
                Contents = $$"""
                    namespace {{ctx.Config.RootNamespace}};

                    /// <summary>
                    /// クエリやコマンドを呼び出したプレゼンテーション層（Web、バッチ、単体テストなど）が、
                    /// 1回の呼び出しについて処理に伝える情報。
                    /// プレゼンテーション層ごとに実装する。プレゼンテーション層固有の情報は実装クラスに自由に持たせてよい。
                    /// </summary>
                    public interface {{INTERFACE}} {

                        /// <summary>
                        /// コマンドについて、入力チェックのみを行うか、それとも実際の更新処理も行うかを表す。
                        ///
                        /// Webアプリケーションで何らかのデータを更新する場合、
                        /// 1回目のHTTPリクエストでは入力チェックのみを行い、ユーザーに確認メッセージを表示した上で
                        /// 2回目のHTTPリクエストで再度入力チェックしたうえで実際の更新処理を行う、という2段階の処理を行うことが多い。
                        /// このプロパティが true の場合、入力チェックのみを行い、実際の更新処理は行わない。
                        /// クエリはデータを変更しないため、このプロパティを参照しない。
                        /// </summary>
                        bool ValidationOnly { get; }
                    }
                    """,
            };
        }
    }
}
