using Microsoft.EntityFrameworkCore;

namespace MyApp;

partial class OverridedApplicationService {

    /// <summary>
    /// 既読になった通知を残しておく期間。これを過ぎたものはクリーンナップで削除する。
    /// 未読の通知は期間によらず削除しない。
    /// </summary>
    public static readonly TimeSpan 既読の通知の保存期間 = TimeSpan.FromDays(90);

    /// <summary>
    /// ユーザー宛ての通知を作る。宛先1人につき1件作る。
    /// 業務のデータと一緒に確定させるため、開始したトランザクションの中で呼ぶこと。
    /// 業務の処理が取り消されれば、通知も残らない。
    /// </summary>
    /// <param name="宛先一覧">宛先のユーザーの従業員番号</param>
    /// <param name="本文">通知の本文</param>
    /// <param name="リンク先">関係する画面の URL のパス。"/" で始まる、このアプリケーションの中のパスに限る</param>
    /// <exception cref="InvalidOperationException">トランザクションが開始されていない場合</exception>
    /// <exception cref="ArgumentException">リンク先がこのアプリケーションの中のパスでない場合や、本文などが通知として保存できない場合</exception>
    public async Task Create通知Async(IEnumerable<string> 宛先一覧, string 本文, string? リンク先 = null) {
        if (リンク先 != null && !Isアプリケーションの中のパス(リンク先)) {
            throw new ArgumentException($"通知のリンク先は、このアプリケーションの中の URL のパスで指定してください: {リンク先}", nameof(リンク先));
        }

        var commands = new List<ValidatedCommand<通知UpdateCommand>>();
        foreach (var 宛先 in 宛先一覧.Distinct()) {
            var command = DbContext.StartCreating通知();
            command.通知ID = Guid.NewGuid().ToString();
            command.宛先 = 宛先;
            command.本文 = 本文;
            command.リンク先 = リンク先;

            // 通知の内容は業務処理のプログラムが決めるので、保存できない内容は利用者の入力ではなくプログラムの誤り
            if (!command.TryValidate(out var validated, out var errors)) {
                throw new ArgumentException($"通知として保存できません: {string.Join(", ", errors.GetAllMessages())}");
            }
            commands.Add(validated);
        }
        if (commands.Count == 0) return;

        var result = await DbContext.TrySaveAsync(commands);
        if (!result.Success) {
            throw new InvalidOperationException($"通知を保存できませんでした: {string.Join(", ", result.Errors.GetAllMessages())}");
        }
    }

    /// <summary>
    /// 通知をログインユーザー宛てのものに絞る。
    /// 利用者の操作で通知を参照・変更する処理は、他のユーザー宛ての通知を扱わないよう、これで絞ること。
    /// </summary>
    /// <exception cref="InvalidOperationException">ログインしていない場合</exception>
    private IQueryable<通知DbEntity> Filter自分宛て(IQueryable<通知DbEntity> query) {
        var 従業員番号 = LoginUser?.従業員番号 ?? throw new InvalidOperationException("ログインしていない状態で、ログインユーザー宛ての通知が参照されました。");
        return query.Where(n => n.宛先 == 従業員番号);
    }

    /// <summary>
    /// クリーンナップで、不要になった通知を削除する。
    /// 既読になってから保存期間が過ぎたものと、宛先のユーザーが削除されたものが対象。
    /// 宛先はユーザーへの外部参照ではないので、ユーザーを削除しても通知は残る。未読のものは他に削除される機会がないので、ここで削除する。
    /// </summary>
    /// <returns>削除した件数</returns>
    private async Task<int> Delete不要な通知Async() {
        var threshold = CurrentTime - 既読の通知の保存期間;
        return await DbContext.通知DbSet
            .Where(n => n.既読日時 < threshold
                     || !DbContext.従業員DbSet.Any(e => e.従業員番号 == n.宛先))
            .ExecuteDeleteAsync();
    }

    /// <summary>
    /// このアプリケーションの中の URL のパスかどうか。
    /// "//" や "/\" で始まるものは、ブラウザが別のサイトの URL として扱うので除く。
    /// 保存した通知のリンクから、利用者が別のサイトへ誘導されないようにするため。
    /// </summary>
    private static bool Isアプリケーションの中のパス(string path) {
        return path.StartsWith('/')
            && !path.StartsWith("//")
            && !path.StartsWith("/\\");
    }
}
