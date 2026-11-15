using System.Text;

namespace MyApp.Core.運用担当への通知;

/// <summary>
/// エラーの通知のメールのモック。メールを送る代わりに、宛先・件名・本文を書いたテキストファイルを、1通につき1つ作る。
/// </summary>
public sealed class ErrorMailSenderMock(string outputDirectory) : IErrorMailSender {
    public async Task SendAsync(ErrorMail mail, CancellationToken cancellationToken) {
        Directory.CreateDirectory(outputDirectory);
        var fileName = $"{DateTime.Now:yyyyMMdd-HHmmss-fff}_{Guid.NewGuid():N}.txt";
        var contents = $"""
            宛先: {string.Join(", ", mail.To)}
            件名: {mail.Subject}

            {mail.Body}
            """;
        await File.WriteAllTextAsync(Path.Combine(outputDirectory, fileName), contents, Encoding.UTF8, cancellationToken);
    }
}
