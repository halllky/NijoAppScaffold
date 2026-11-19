using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyApp.Core.添付ファイル;

namespace MyApp;

partial class OverridedApplicationService {

    /// <summary>
    /// 一時保存したファイルを、どのデータにも添付されないまま残しておく期間。これを過ぎたものはクリーンナップで削除する。
    /// </summary>
    public static readonly TimeSpan 添付ファイルの一時保存の期間 = TimeSpan.FromHours(24);

    /// <summary>
    /// ファイルを一時保存する。一時保存したファイルは、データに添付して保存すると確定する。
    /// 画面からは一時保存の API を経由して呼ばれる。ログインユーザーのいない処理（ジョブなど）からは直接呼んでよい。
    /// 中で独自にトランザクションを開始してコミットするので、トランザクションの外で呼ぶこと。
    /// </summary>
    /// <param name="名前">ファイル名。パスが含まれていればファイル名の部分だけを使う</param>
    /// <param name="種類">Content-Type。分からなければ null</param>
    /// <param name="中身">ファイルの中身</param>
    /// <param name="messages">サイズの上限を超えたときなどのエラーの出力先</param>
    /// <returns>一時保存したファイル。エラーがあれば null</returns>
    /// <exception cref="InvalidOperationException">トランザクションの中で呼んだ場合</exception>
    public async Task<一時保存した添付ファイル?> 一時保存添付ファイルAsync(string 名前, string? 種類, Stream 中身, MessageSetter messages, CancellationToken ct = default) {
        var 識別子 = Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(32));
        var maxBytes = Settings.Attachment.MaxFileSizeBytes;

        // 中身を先に書く。管理テーブルの行より先にファイルがあれば、行があるのにファイルが無い状態は起きない
        var counter = new サイズ上限付きStream(中身, maxBytes);
        try {
            await 添付ファイル保存先.SaveAsync(識別子, counter, ct);
        } catch (サイズ上限付きStream.上限超過Exception) {
            messages.AddError(MSG.AttachmentTooLargeError($"{Settings.Attachment.MaxFileSizeMegabytes}MB"));
            return null;
        }

        try {
            var command = DbContext.StartCreating添付ファイル();
            command.識別子 = 識別子;
            command.名前 = ファイル名の部分(名前);
            command.サイズ = (int)counter.ReadBytes;
            command.種類 = string.IsNullOrWhiteSpace(種類) ? "application/octet-stream" : 種類;
            command.状態 = 添付ファイルの状態.一時保存;

            if (!command.TryValidate(out var validated, out var errors)) {
                foreach (var message in errors.GetAllMessages()) messages.AddError(message);
                await 添付ファイル保存先.DeleteAsync(識別子, CancellationToken.None);
                return null;
            }

            await using var tran = await BeginTransactionAsync();
            var result = await DbContext.TrySaveAsync(validated);
            if (!result.Success) {
                foreach (var message in result.Errors.GetAllMessages()) messages.AddError(message);
                await 添付ファイル保存先.DeleteAsync(識別子, CancellationToken.None);
                return null;
            }
            await tran.CommitAsync();

            return new 一時保存した添付ファイル(識別子, command.名前!, command.サイズ.Value, command.種類);

        } catch {
            await 添付ファイル保存先.DeleteAsync(識別子, CancellationToken.None);
            throw;
        }
    }

    /// <summary>
    /// 添付ファイルの中身を読み出す。処理の中でファイルの中身が必要なときに使う。呼び出し元が閉じる。
    /// 状態や権限は確かめないので、利用者にファイルを返すときはダウンロードの処理を使う。
    /// </summary>
    /// <exception cref="FileNotFoundException">保存先にファイルが無い場合</exception>
    public async Task<Stream> Open添付ファイルAsync(string 識別子, CancellationToken ct = default) {
        return await 添付ファイル保存先.OpenReadAsync(識別子, ct)
            ?? throw new FileNotFoundException($"添付ファイルが保存先にありません: {識別子}");
    }

    /// <summary>
    /// ログインユーザーがダウンロードできる添付ファイルを探す。
    /// 確定したファイルは誰でも、一時保存のファイルは一時保存したユーザーだけがダウンロードできる。参照なしのファイルはダウンロードできない。
    /// ダウンロードできないファイルは、存在しないファイルと同じように扱い、存在すること自体を知らせない。
    /// </summary>
    /// <returns>ダウンロードできるファイルが無ければ null。中身は呼び出し元が閉じる</returns>
    public async Task<ダウンロードする添付ファイル?> Findダウンロードできる添付ファイルAsync(string 識別子, CancellationToken ct = default) {
        var file = await DbContext.添付ファイルDbSet.SingleOrDefaultAsync(f => f.識別子 == 識別子, ct);
        if (file == null) return null;

        var ダウンロードできる = file.状態 switch {
            添付ファイルの状態.確定 => true,
            添付ファイルの状態.一時保存 => LoginUser != null && file.CreateUser == LoginUser.従業員番号,
            _ => false,
        };
        if (!ダウンロードできる) return null;
        if (!await 添付ファイルをダウンロードしてよいかAsync(file)) return null;

        var 中身 = await 添付ファイル保存先.OpenReadAsync(識別子, ct);
        if (中身 == null) {
            Log.LogWarning("管理テーブルに行がある添付ファイルが、保存先にありません: {識別子}", 識別子);
            return null;
        }
        return new ダウンロードする添付ファイル(中身, file.名前!, file.種類!);
    }

    /// <summary>
    /// 案件ごとの、ダウンロードしてよいかの確認。既定では確認しない。
    /// 「そのファイルが添付されたデータを見る権限があるか」で判断する場合は、ここで、そのファイルを参照しているテーブルを検索する。
    /// その場合は、データに添付できるファイルも同じ条件で絞ること。絞らないと、自分のデータに添付してからダウンロードできてしまう。
    /// </summary>
    private Task<bool> 添付ファイルをダウンロードしてよいかAsync(添付ファイルDbEntity file) {
        return Task.FromResult(true);
    }

    /// <summary>
    /// パスからファイル名の部分を取り出す。古いブラウザは "C:\fakepath\a.txt" のようにパスを付けて送ってくるので、
    /// サーバーの OS によらず "/" と "\" の両方を区切りとして扱う。
    /// </summary>
    private static string ファイル名の部分(string 名前) {
        return 名前[(名前.LastIndexOfAny(['/', '\\']) + 1)..];
    }

    /// <summary>
    /// 添付ファイルの保存先
    /// </summary>
    private I添付ファイル保存先 添付ファイル保存先 => ServiceProvider.GetRequiredService<I添付ファイル保存先>();

    /// <summary>
    /// 読み出したバイト数を数え、上限を超えたら例外を投げる。
    /// リクエストの Content-Length などを信用せず、実際に読んだ量で上限を確かめるため。
    /// </summary>
    private sealed class サイズ上限付きStream(Stream inner, long maxBytes) : Stream {
        public long ReadBytes { get; private set; }

        public override int Read(byte[] buffer, int offset, int count) => Count(inner.Read(buffer, offset, count));
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => Count(await inner.ReadAsync(buffer, cancellationToken));
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => Count(await inner.ReadAsync(buffer.AsMemory(offset, count), cancellationToken));

        private int Count(int read) {
            ReadBytes += read;
            if (ReadBytes > maxBytes) throw new 上限超過Exception();
            return read;
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => ReadBytes; set => throw new NotSupportedException(); }
        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public sealed class 上限超過Exception : Exception;
    }
}

/// <summary>
/// 一時保存したファイルの情報。画面は、これを入力中のデータに入れ、処理を呼ぶときに識別子を送る。
/// </summary>
public sealed record 一時保存した添付ファイル(string 識別子, string 名前, int サイズ, string 種類);

/// <summary>
/// ダウンロードするファイルの中身と、応答に付ける名前と種類。中身は受け取った側が閉じる。
/// </summary>
public sealed record ダウンロードする添付ファイル(Stream 中身, string 名前, string 種類);
