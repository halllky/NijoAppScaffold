using Nijo.CodeGenerating;

namespace Nijo.Models.WriteModel2Modules {
    /// <summary>
    /// ナビゲーションプロパティ生成を担当する移植先。
    /// </summary>
    internal abstract class NavigationProperty {
        // TODO: 実装時は旧版の「親子」「ref-to」の2系統を表す具象クラスをこのファイル内に定義する。
        // TODO: 可能なら現行 Nijo.Parts.CSharp.EFCoreNavigationProperty の Principal/Relevant モデルへ寄せ、
        // TODO: WriteModel2 固有差分だけをここに残す。

        /// <summary>
        /// ナビゲーションプロパティ宣言をレンダリングする。
        /// </summary>
        /// <remarks>
        /// 実装方針: 旧版 NavigationProperty が持っていた親子・参照の両パターンを、現行 EFCoreEntity 設計に沿って分岐実装する。
        /// </remarks>
        internal abstract string RenderDeclaring(CodeRenderingContext ctx);

        /// <summary>
        /// OnModelCreating 用の Fluent API をレンダリングする。
        /// </summary>
        /// <remarks>
        /// 実装方針: 外部キー制約名、DeleteBehavior、多重度の決定をここへ閉じ込める。
        /// 親子と ref-to で Principal / Relevant の向きが異なるため、その判定責務も具象クラス側に持たせる。
        /// </remarks>
        internal abstract string RenderOnModelCreating(CodeRenderingContext ctx);
    }
}
