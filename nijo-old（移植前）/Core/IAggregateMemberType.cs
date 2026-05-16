using Nijo.Util.CodeGenerating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Core {
    internal interface IAggregateMemberType {
        /// <summary>
        /// nijo ui の画面上に表示される名前
        /// </summary>
        string GetUiDisplayName();
        /// <summary>
        /// 説明文。このメンバー型がどういった種類のデータを表すのか、
        /// 代表的な挙動や特徴的な挙動はどういったものなのかを記載してください。
        /// </summary>
        string GetHelpText();

        string GetCSharpTypeName();
        string GetTypeScriptTypeName();

        WijmoGridColumnSetting GetWijmoGridColumnSetting();
        /// <summary>
        /// コード自動生成時に呼ばれる。C#の列挙体の定義を作成するなどの用途を想定している。
        /// </summary>
        void GenerateCode(CodeRenderingContext context) { }

        string GetSearchConditionCSharpType(AggregateMember.ValueMember vm);
        string GetSearchConditionTypeScriptType(AggregateMember.ValueMember vm);

        /// <summary>
        /// 検索条件の絞り込み処理（WHERE句組み立て処理）をレンダリングします。
        /// </summary>
        /// <param name="member">検索対象のメンバーの情報</param>
        /// <param name="query"> <see cref="IQueryable{T}"/> の変数の名前</param>
        /// <param name="searchCondition">検索処理のパラメータの値の変数の名前</param>
        /// <param name="searchConditionObject">検索条件のオブジェクトの型</param>
        /// <param name="searchQueryObject">検索結果のクエリのオブジェクトの型</param>
        /// <returns> <see cref="IQueryable{T}"/> の変数に絞り込み処理をつけたものを再代入するソースコード</returns>
        string RenderFilteringStatement(AggregateMember.ValueMember member, string query, string searchCondition, E_SearchConditionObject searchConditionObject, E_SearchQueryObject searchQueryObject);
        /// <summary>
        /// 検索条件欄のUI（"VerticalForm.Item" の子要素）をレンダリングします。
        /// </summary>
        /// <param name="vm">検索対象のメンバーの情報</param>
        /// <param name="ctx">コンテキスト引数</param>
        /// <param name="searchConditionObject">検索条件のオブジェクトの型</param>
        string RenderSearchConditionVFormBody(AggregateMember.ValueMember vm, FormUIRenderingContext ctx);
        /// <summary>
        /// 詳細画面のUI（"VerticalForm.Item" の子要素）をレンダリングします。
        /// </summary>
        /// <param name="vm">検索対象のメンバーの情報</param>
        /// <param name="ctx">コンテキスト引数</param>
        string RenderSingleViewVFormBody(AggregateMember.ValueMember vm, FormUIRenderingContext ctx);

        /// <summary>
        /// ソース生成後プロジェクトでカスタマイズされた検索条件UIを使用する場合の、当該検索条件UIのReactコンポーネントのpropsのうち、
        /// どのコンポーネントでも必要な共通のプロパティを除いたものを "プロパティ名: 型名" の配列の形で列挙します。
        /// 未指定の場合はそういった追加のプロパティは特になし。
        /// </summary>
        IEnumerable<string> EnumerateSearchConditionCustomFormUiAdditionalProps() => [];
        /// <summary>
        /// ソース生成後プロジェクトでカスタマイズされた詳細画面フォームUIを使用する場合の、当該詳細画面フォームUIのReactコンポーネントのpropsのうち、
        /// どのコンポーネントでも必要な共通のプロパティを除いたものを "プロパティ名: 型名" の配列の形で列挙します。
        /// 未指定の場合はそういった追加のプロパティは特になし。
        /// </summary>
        IEnumerable<string> EnumerateSingleViewCustomFormUiAdditionalProps() => [];

        /// <summary>
        /// <see cref="Parts.WebClient.DataTable.CellType"/> で使用される列定義生成ヘルパーメソッドの名前
        /// </summary>
        string DataTableColumnDefHelperName { get; }

        /// <summary>
        /// UIの制約定義（文字列項目なら最大文字数など、数値なら桁数など）の型の名前。
        /// </summary>
        string UiConstraintType { get; }
        /// <summary>
        /// UIの制約定義（文字列項目なら最大文字数など、数値なら桁数など）の具体的な値をレンダリングします。
        /// なお、必須項目か否かはこのメソッドを呼ぶ側でレンダリングしているため、定義不要です。
        /// </summary>
        IEnumerable<string> RenderUiConstraintValue(AggregateMember.ValueMember vm);
    }

    /// <summary>検索条件のオブジェクトの型</summary>
    internal enum E_SearchConditionObject {
        /// <summary>検索条件の型は <see cref="Models.ReadModel2Features.SearchCondition"/> </summary>
        SearchCondition,
        /// <summary>検索条件の型は <see cref="Models.RefTo.RefSearchCondition"/> </summary>
        RefSearchCondition,
    }
    /// <summary>検索結果のクエリのオブジェクトの型</summary>
    internal enum E_SearchQueryObject {
        /// <summary>クエリのオブジェクトの型は <see cref="Models.WriteModel2Features.EFCoreEntity"/> </summary>
        EFCoreEntity,
        /// <summary>クエリのオブジェクトの型は <see cref="Models.ReadModel2Features.SearchResult"/> </summary>
        SearchResult,
    }

    /// <summary>
    /// WijmoのFlexGrid用の設定
    /// </summary>
    public class WijmoGridColumnSetting {
        /// <summary>
        /// セル型
        /// </summary>
        public required string DataType { get; init; }
        /// <summary>
        /// セルの書式設定
        /// </summary>
        public required string? Format { get; init; }

    }
}
