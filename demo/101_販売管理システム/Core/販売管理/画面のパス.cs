namespace MyApp;

/// <summary>
/// 販売管理の画面の URL のパス。サーバーで組み立てる通知のリンク先に使う。
/// 画面の URL は client の各画面のファイルが決めているので、それと同じ形にする。画面の URL を変えたら、ここも直すこと。
/// 変える前に作った通知のリンクは切れる。
/// </summary>
internal static class 画面のパス {
    /// <summary>P101 売上詳細</summary>
    public static string 売上詳細(int 売上SEQ) => $"/uriage/edit/{売上SEQ}";
    /// <summary>P301 商品詳細</summary>
    public static string 商品詳細(int 商品SEQ) => $"/shohin/{商品SEQ}";
}
