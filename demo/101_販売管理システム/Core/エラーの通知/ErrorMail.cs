namespace MyApp.Core.エラーの通知;

/// <summary>
/// エラーの通知のメール1通。
/// </summary>
public sealed record ErrorMail(IReadOnlyList<string> To, string Subject, string Body);
